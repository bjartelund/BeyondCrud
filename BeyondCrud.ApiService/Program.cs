using BeyondCrud.Domain.Commands;
using BeyondCrud.Grains;
using BeyondCrud.Infrastructure;
using BeyondCrud.Projections;
using BeyondCrud.ServiceDefaults;
using Orleans.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Talk to the silo. With development clustering Aspire gives the client no gateway,
// so AppHost passes the silo's gateway port and we point the client at it here.
builder.UseOrleansClient(client =>
{
    if (builder.Configuration.GetValue<int?>("Orleans:Endpoints:GatewayPort") is { } gatewayPort)
    {
        client.UseLocalhostClustering(
            [gatewayPort],
            builder.Configuration["Orleans:ServiceId"] ?? "dev",
            builder.Configuration["Orleans:ClusterId"] ?? "dev");
    }
});
builder.Services.AddSerializer(serializer => serializer.AddDomainSerialization());

// Add services to the container.
builder.Services.AddProblemDetails();

// The read side: follows the event streams and keeps an in-memory flight list up to date.
builder.Services.AddEventStoreDb(
    builder.Configuration.GetConnectionString("eventstore")
    ?? throw new InvalidOperationException("Missing connection string 'eventstore'."));
builder.Services.AddFlightProjections();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


app.MapGet("/", () => "API service is running. POST to /demo to see sample data.");

app.MapPost("/demo", async (string name, IGrainFactory grains) =>
{
    var flightId = Guid.CreateVersion7();
    var flight = grains.GetGrain<IFlightGrain>(flightId);

    await flight.Create(new CreateFlight(flightId, "Oslo", 100));
    await flight.OpenRegistration(new OpenRegistration(flightId));
    await flight.RegisterPassenger(new RegisterPassenger(flightId, Guid.CreateVersion7(), name));

    return await flight.GetSummary();
});

app.MapPost("/flights", async (string destination, int? capacity, IGrainFactory grains) =>
{
    var flightId = Guid.CreateVersion7();

    await Rejectable(() => grains.GetGrain<IFlightGrain>(flightId)
        .Create(new CreateFlight(flightId, destination, capacity ?? 100)));

    return Results.Created($"/flights/{flightId}", new { flightId });
});

app.MapGet("/flights/{flightId:guid}", async (Guid flightId, IGrainFactory grains)
    => await grains.GetGrain<IFlightGrain>(flightId).GetSummary());

// Queries go to the read model, commands go to the grains. The read model is eventually consistent.
app.MapGet("/read/flights", (FlightListReadModel readModel)
    => readModel.List());

app.MapGet("/read/flights/{flightId:guid}", (Guid flightId, FlightListReadModel readModel)
    => readModel.Get(flightId) is { } flight ? Results.Ok(flight) : Results.NotFound());

app.MapPost("/flights/{flightId:guid}/registration/open", (Guid flightId, IGrainFactory grains)
    => Rejectable(() => grains.GetGrain<IFlightGrain>(flightId)
        .OpenRegistration(new OpenRegistration(flightId))));

app.MapPost("/flights/{flightId:guid}/registration/close", (Guid flightId, IGrainFactory grains)
    => Rejectable(() => grains.GetGrain<IFlightGrain>(flightId)
        .CloseRegistration(new CloseRegistration(flightId))));

app.MapPost("/flights/{flightId:guid}/passengers", async (Guid flightId, string name, IGrainFactory grains) =>
{
    var passengerId = Guid.CreateVersion7();

    var result = await Rejectable(() => grains.GetGrain<IFlightGrain>(flightId)
        .RegisterPassenger(new RegisterPassenger(flightId, passengerId, name)));

    return result is IStatusCodeHttpResult { StatusCode: >= 400 } ? result : Results.Ok(new { passengerId });
});

app.MapPost("/flights/{flightId:guid}/passengers/{passengerId:guid}/seat",
    (Guid flightId, Guid passengerId, string seat, IGrainFactory grains)
        => Rejectable(() => grains.GetGrain<IFlightGrain>(flightId)
            .AssignSeat(new AssignSeat(flightId, passengerId, seat))));

app.MapDefaultEndpoints();

app.Run();

// Domain rules are the client's problem, not a server fault.
static async Task<IResult> Rejectable(Func<Task> command)
{
    try
    {
        await command();
        return Results.NoContent();
    }
    catch (CommandRejectedException exception)
    {
        return Results.Conflict(new { error = exception.Message });
    }
}
using BeyondCrud.Domain;
using BeyondCrud.Domain.Commands;
using BeyondCrud.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

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

app.MapPost("/demo", (string name) =>
{
    var flightId = Guid.CreateVersion7();
    var flight = new Flight();

    flight.Create(new CreateFlight(flightId, "Oslo", 100));
    flight.OpenRegistration(new OpenRegistration(flightId));
    flight.RegisterPassenger(new RegisterPassenger(flightId, Guid.CreateVersion7(), name));

    return flight.GetUncommittedEvents();
});

app.MapDefaultEndpoints();

app.Run();
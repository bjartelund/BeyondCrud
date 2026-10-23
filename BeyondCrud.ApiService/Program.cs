using BeyondCrud.Domain;
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
    var passenger = new Passenger();

    passenger.Register(
        Guid.CreateVersion7(),
        name);

    return passenger.GetUncommittedEvents();
});

app.MapDefaultEndpoints();

app.Run();
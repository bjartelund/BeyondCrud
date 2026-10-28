using BeyondCrud.Grains;
using BeyondCrud.ServiceDefaults;
using System.Net;
using Orleans.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.UseOrleans(silo =>
{
    // Development clustering is single machine: bind loopback so the client (via Aspire's proxy) can reach us.
    var siloPort = builder.Configuration.GetValue<int?>("Orleans:Endpoints:SiloPort");
    var gatewayPort = builder.Configuration.GetValue<int?>("Orleans:Endpoints:GatewayPort");

    if (siloPort is { } silos && gatewayPort is { } gateways)
        silo.ConfigureEndpoints(IPAddress.Loopback, silos, gateways);
});
builder.Services.AddSerializer(serializer => serializer.AddDomainSerialization());

var app = builder.Build();

app.MapDefaultEndpoints();

app.Run();
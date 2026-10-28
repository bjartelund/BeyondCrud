var builder = DistributedApplication.CreateBuilder(args);

// Development clustering, single silo: fixed loopback ports. Orleans gateways can't sit behind
// Aspire's proxy, so the silo and its client agree on the real port instead of discovering it.
const string siloPort = "41111";
const string gatewayPort = "41112";

var orleans = builder.AddOrleans("cluster")
    .WithDevelopmentClustering();

var silo = builder.AddProject<Projects.BeyondCrud_Silo>("silo")
    .WithReference(orleans)
    .WithEnvironment("Orleans__Endpoints__SiloPort", siloPort)
    .WithEnvironment("Orleans__Endpoints__GatewayPort", gatewayPort)
    .WithHttpHealthCheck("/health");

var apiService = builder.AddProject<Projects.BeyondCrud_ApiService>("apiservice")
    .WithReference(orleans.AsClient())
    .WithEnvironment("Orleans__Endpoints__GatewayPort", gatewayPort)
    .WaitFor(silo)
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.BeyondCrud_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
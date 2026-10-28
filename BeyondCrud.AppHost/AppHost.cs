var builder = DistributedApplication.CreateBuilder(args);

// Development clustering, single silo: fixed loopback ports. Orleans gateways can't sit behind
// Aspire's proxy, so the silo and its client agree on the real port instead of discovering it.
const string siloPort = "41111";
const string gatewayPort = "41112";

var orleans = builder.AddOrleans("cluster")
    .WithDevelopmentClustering();

var eventStore = builder.AddContainer("eventstore", "eventstore/eventstore", "24.10")
    .WithHttpEndpoint(port: 2113, targetPort: 2113, name: "http")
    .WithEnvironment("EVENTSTORE_CLUSTER_SIZE", "1")
    .WithEnvironment("EVENTSTORE_RUN_PROJECTIONS", "All")
    .WithEnvironment("EVENTSTORE_START_STANDARD_PROJECTIONS", "true")
    .WithEnvironment("EVENTSTORE_INSECURE", "true")
    .WithEnvironment("EVENTSTORE_ENABLE_ATOM_PUB_OVER_HTTP", "true")
    .WithVolume("beyondcrud-eventstore-data", "/var/lib/eventstore")
    .WithHttpHealthCheck("/health/live", statusCode: 204, endpointName: "http")
    .WithUrlForEndpoint("http", url => url.DisplayText = "EventStoreDB UI");

var eventStoreEndpoint = eventStore.GetEndpoint("http");

var eventStoreConnectionString = ReferenceExpression.Create(
    $"esdb://{eventStoreEndpoint.Property(EndpointProperty.Host)}:{eventStoreEndpoint.Property(EndpointProperty.Port)}?tls=false");

var silo = builder.AddProject<Projects.BeyondCrud_Silo>("silo")
    .WithReference(orleans)
    .WithEnvironment("Orleans__Endpoints__SiloPort", siloPort)
    .WithEnvironment("Orleans__Endpoints__GatewayPort", gatewayPort)
    .WithEnvironment("ConnectionStrings__eventstore", eventStoreConnectionString)
    .WaitFor(eventStore)
    .WithHttpHealthCheck("/health");

var apiService = builder.AddProject<Projects.BeyondCrud_ApiService>("apiservice")
    .WithReference(orleans.AsClient())
    .WithEnvironment("Orleans__Endpoints__GatewayPort", gatewayPort)
    .WithEnvironment("ConnectionStrings__eventstore", eventStoreConnectionString)
    .WaitFor(silo)
    .WaitFor(eventStore)
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.BeyondCrud_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;
using Orleans.Hosting;
using Orleans.Serialization;
using Orleans.TestingHost;
using TUnit.Core.Interfaces;

namespace BeyondCrud.Grains.Tests;

public class ClusterFixture : IAsyncInitializer, IAsyncDisposable
{
    public TestCluster Cluster { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var builder = new TestClusterBuilder();
        builder.AddSiloBuilderConfigurator<SiloConfigurator>();
        builder.AddClientBuilderConfigurator<ClientConfigurator>();

        Cluster = builder.Build();
        await Cluster.DeployAsync();
    }

    public async ValueTask DisposeAsync()
        => await Cluster.StopAllSilosAsync();

    private class SiloConfigurator : ISiloConfigurator
    {
        public void Configure(ISiloBuilder siloBuilder)
            => siloBuilder.Services.AddSerializer(serializer => serializer.AddDomainSerialization());
    }

    private class ClientConfigurator : IClientBuilderConfigurator
    {
        public void Configure(IConfiguration configuration, IClientBuilder clientBuilder)
            => clientBuilder.Services.AddSerializer(serializer => serializer.AddDomainSerialization());
    }
}
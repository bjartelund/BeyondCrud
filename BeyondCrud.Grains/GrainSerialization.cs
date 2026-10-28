using Orleans.Serialization;

namespace BeyondCrud.Grains;

public static class GrainSerialization
{
    /// <summary>
    /// The domain library has no Orleans dependency, so its commands and events carry no
    /// serialization attributes. System.Text.Json handles everything in our own namespaces.
    /// Exceptions are left to Orleans, which has its own codec for them.
    /// </summary>
    public static ISerializerBuilder AddDomainSerialization(this ISerializerBuilder builder)
        => builder.AddJsonSerializer(type =>
            type.Namespace?.StartsWith("BeyondCrud") == true
            && !typeof(Exception).IsAssignableFrom(type));
}
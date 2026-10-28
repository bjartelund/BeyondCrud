using System.Text.Json;
using BeyondCrud.Domain;

namespace BeyondCrud.Infrastructure;

/// <summary>
/// Events are stored as JSON, tagged with the record's name. Renaming an event type is a breaking change.
/// </summary>
public static class EventSerializer
{
    private static readonly IReadOnlyDictionary<string, Type> EventTypes =
        typeof(IDomainEvent).Assembly
            .GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false } && typeof(IDomainEvent).IsAssignableFrom(type))
            .ToDictionary(type => type.Name);

    public static (string EventType, byte[] Data) Serialize(IDomainEvent @event)
        => (@event.GetType().Name, JsonSerializer.SerializeToUtf8Bytes(@event, @event.GetType()));

    public static IDomainEvent Deserialize(string eventType, ReadOnlyMemory<byte> data)
    {
        if (!EventTypes.TryGetValue(eventType, out var type))
            throw new InvalidOperationException($"Unknown event type '{eventType}'.");

        return (IDomainEvent)JsonSerializer.Deserialize(data.Span, type)!;
    }
}
namespace BeyondCrud.Infrastructure;

public class ConcurrencyException(string streamName, long expectedVersion, Exception? innerException = null)
    : Exception($"Stream '{streamName}' was not at expected version {expectedVersion}.", innerException);
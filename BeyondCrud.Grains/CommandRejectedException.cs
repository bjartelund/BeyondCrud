namespace BeyondCrud.Grains;

/// <summary>
/// What callers of a grain see when the aggregate rejects a command.
/// The domain's own exception can't cross the grain boundary without an Orleans dependency.
/// </summary>
[GenerateSerializer]
public sealed class CommandRejectedException : Exception
{
    public CommandRejectedException()
    {
    }

    public CommandRejectedException(string message) : base(message)
    {
    }

    public CommandRejectedException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

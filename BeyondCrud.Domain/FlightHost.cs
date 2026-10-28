using System.Threading.Channels;

namespace BeyondCrud.Domain;

/// <summary>
/// A tiny hand-rolled actor: owns one <see cref="Flight"/> and a mailbox.
/// Callers never touch the flight directly, they send messages and await the reply.
/// </summary>
public sealed class FlightHost
{
    private readonly Flight _flight = new();

    private readonly Channel<Action<Flight>> _mailbox =
        Channel.CreateUnbounded<Action<Flight>>(new UnboundedChannelOptions { SingleReader = true });

    public FlightHost()
    {
        _ = Task.Run(ProcessMessages);
    }

    public Task<T> Ask<T>(Func<Flight, T> message)
    {
        var reply = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);

        _mailbox.Writer.TryWrite(flight =>
        {
            try
            {
                reply.SetResult(message(flight));
            }
            catch (Exception exception)
            {
                reply.SetException(exception);
            }
        });

        return reply.Task;
    }

    public Task Tell(Action<Flight> message)
        => Ask(flight =>
        {
            message(flight);
            return true;
        });

    private async Task ProcessMessages()
    {
        // TODO: read messages one at a time from the mailbox and handle each against _flight
        await Task.CompletedTask;
    }
}
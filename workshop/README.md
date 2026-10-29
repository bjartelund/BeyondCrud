# Beyond CRUD: workshop guide

How do we model a business process that unfolds over time? Each step is a git tag. Check out the tag, do the
exercise, and compare with the matching `-solution` tag.

```
git checkout 01-process                          # the starting point for a step
git diff 01-process 01-process-solution          # what the exercise adds
git checkout 01-process-solution                 # skip ahead if you get lost
```

Every tag builds. The only failures are the exercise's own red tests; solution tags are fully green.
Steps 06 onwards use Docker (EventStoreDB), and every exercise test still runs without it.

## Before you start

- .NET 10 SDK (`dotnet --version` should say 10.x) and git.
- Docker running, for `dotnet run --project BeyondCrud.AppHost` (steps 06 to 08). Pull the image beforehand so the
  first run does not wait on the network: `docker pull eventstore/eventstore:24.10`.
- Clone fresh. The tags were rewritten once; an old clone keeps the old ones until `git fetch --tags --force`.
- Time: about 2.5 to 3.5 hours self-paced, 4 to 5 hours facilitated with discussion and breaks (see the table).

The code to write is small, 2 to 19 lines per step. Most of the time goes into reading the existing code and the
discussion, so the times below are mostly not typing.

| Tag | Question | Exercise | Red until done | Self-paced |
|---|---|---|---|---|
| `start` | What is an event? | Emit `PassengerRegistered` in `Passenger.Register` | `PassengerTests` (`Domain.Tests`) | 10 to 15 min |
| `01-process` | What does the business process look like as events? | `Flight.OpenRegistration` and `CloseRegistration` | 2 in `FlightTests` (`Domain.Tests`) | 10 to 15 min |
| `02-event-sourcing` | Can state be rebuilt from events alone? | The `Apply` switch arms in `Flight` | 2 `Rehydrate_*` tests (`Domain.Tests`) | 15 to 20 min |
| `03-invariants` | Who decides whether a command is valid? | Guard clauses in `RegisterPassenger` and `AssignSeat` | 6 rejection tests (`Domain.Tests`) | 20 to 30 min |
| `04-actor` | Can two people change a flight at once? | The single-reader loop in `FlightHost` | `FlightHostTests` in `Domain.Tests` (time out) | 15 to 25 min |
| `05-orleans` | How do we host aggregates? | `FlightGrain.RegisterPassenger` and `CloseRegistration` | 2 in `BeyondCrud.Grains.Tests` | 15 to 20 min |
| `06-eventstoredb` | Where do the events live? | `FlightGrain` activation and `Execute` persistence | 3 in `FlightGrainPersistenceTests` (`Grains.Tests`) | 25 to 35 min |
| `07-projections` | How do we answer queries? | `FlightListProjection` handlers | 2 in `BeyondCrud.Projections.Tests` | 15 to 20 min |
| `08-aspire-ui` | Does it all hang together? (capstone) | `Flight.CheckIn` rules | 4 `CheckIn_*` tests (`Domain.Tests`) | 20 to 30 min |

Run the tests with `dotnet run --project BeyondCrud.Domain.Tests` (also `.Grains.Tests`, `.Projections.Tests`).
Run the whole system with `dotnet run --project BeyondCrud.AppHost` (Docker must be running).

## Demo script per step

**start / 01-process**: draw the lifecycle (flight announced, registration opens, passenger registers, ...). Ask:
"If all we have is the current database state, where did the story go?" Show `status = "CheckedIn"` next to
`PassengerCheckedIn`. Live-code the exercise and let the red test guide.

**02-event-sourcing**: events become the source of truth. `Flight.Rehydrate(history)` is the whole idea. Point out
that `Raise` applies an event to state *and* records it, and that `Rehydrate` never records anything.

**03-invariants**: an aggregate exists because something has to decide. Compare the closed-flight rule with
a rule that needs other state (seat taken, flight full).

**04-actor**: two commands arriving together must not interleave. Single-threaded, stateful, addressable: that is
an actor. `FlightHost` is one channel and a four-line loop; the rest is the `Ask`/`Tell` plumbing. Ask what is missing (more than one flight, placement, failure).

**05-orleans**: Orleans as the implementation detail of the previous step. `GetGrain<IFlightGrain>(id)` is the
addressable part. Worth a look: why the domain has no Orleans attributes (see `GrainSerialization`) and why
`DomainException` becomes `CommandRejectedException` at the boundary.

**06-eventstoredb**: one stream per aggregate, `flight-{id}`. Append with an expected version (optimistic
concurrency). Open the EventStoreDB UI from the Aspire dashboard, create a flight, and watch the stream grow.
Stop and restart the AppHost: the flight is still there.

**07-projections**: the read side follows the streams and can be rebuilt from scratch (this one replays everything
on every start). It is eventually consistent; show the short delay in the UI. Discuss checkpoints and persistence.

**08-aspire-ui**: five minutes of infrastructure, not architecture. Show the dashboard (traces from the API through
the grain to EventStoreDB), the volume that keeps the data, and the flight desk. The capstone adds a new fact
(`PassengerCheckedIn`) end to end; only the aggregate rules are left to write.

## Things that bit us (good to know when something is off)

- Orleans gateways cannot sit behind Aspire's proxy, so the AppHost pins the silo (`41111`) and gateway (`41112`)
  ports on loopback. Change them in `BeyondCrud.AppHost/AppHost.cs` if they clash.
- EventStoreDB's `/health/live` returns 204, and the Aspire health check is told to expect that.
- Event type names are stored as the record name. Renaming an event is a breaking change.
- The projection has no checkpoint by design; every start replays history into an empty read model.
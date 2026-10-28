# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

.NET 10 solution (`BeyondCrud.slnx`) orchestrated with .NET Aspire. The project is an early-stage event-sourced domain (not CRUD): aggregates record state changes as domain events rather than mutating stored state.

## Commands

- Run everything (API + Blazor web, with Aspire dashboard): `dotnet run --project BeyondCrud.AppHost`
- Build: `dotnet build BeyondCrud.slnx`
- Run tests: `dotnet run --project BeyondCrud.Domain.Tests` (TUnit on Microsoft.Testing.Platform; the test project is an `Exe`)
- Run grain tests (Orleans TestingHost): `dotnet run --project BeyondCrud.Grains.Tests`
- Single test: `dotnet run --project BeyondCrud.Domain.Tests -- --treenode-filter "/*/*/FlightTests/Create_emits_flight_created_event"`

## Architecture

- `BeyondCrud.AppHost` – Aspire orchestrator; wires `webfrontend` to `apiservice` to `silo` (Orleans development clustering, reference + wait-for, `/health` checks).
- `BeyondCrud.ServiceDefaults` – shared Aspire defaults (telemetry, health, resilience), applied via `AddServiceDefaults()` / `MapDefaultEndpoints()`.
- `BeyondCrud.ApiService` – minimal API and Orleans client; `/flights/...` endpoints talk to `IFlightGrain`, `/demo` runs a small scenario. `CommandRejectedException` maps to HTTP 409.
- `BeyondCrud.Silo` – Orleans silo hosting the grains. AppHost pins silo/gateway ports (development clustering, single silo) because Orleans gateways can't sit behind Aspire's proxy.
- `BeyondCrud.Grains` – `IFlightGrain`/`FlightGrain` wrap the `Flight` aggregate. The domain has no Orleans dependency, so grain contracts are serialized with System.Text.Json (`GrainSerialization`), and `DomainException` is translated to the Orleans-serializable `CommandRejectedException` at the grain boundary.
- `BeyondCrud.Grains.Tests` – grain tests on an in-process Orleans `TestCluster`.
- `BeyondCrud.Web` – Blazor Server frontend.
- `BeyondCrud.Domain` – dependency-free domain library. The `Flight` aggregate raises events through `Raise`, which applies them to state and adds them to an uncommitted list (`GetUncommittedEvents()`); `Flight.Rehydrate(history)` rebuilds state from events. Commands live in `Commands/` (records), events in `Events/` (records implementing `IDomainEvent`), rule violations throw `DomainException`. `FlightHost` is a plain-.NET mailbox actor around one `Flight`. No persistence or event-store yet.
- `BeyondCrud.Domain.Tests` – tests assert on emitted events (Arrange/Act/Assert, async TUnit assertions: `await Assert.That(...)`).

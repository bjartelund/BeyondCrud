# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

.NET 10 solution (`BeyondCrud.slnx`) orchestrated with .NET Aspire. The project is an early-stage event-sourced domain (not CRUD): aggregates record state changes as domain events rather than mutating stored state.

## Commands

- Run everything (API + Blazor web, with Aspire dashboard): `dotnet run --project BeyondCrud.AppHost`
- Build: `dotnet build BeyondCrud.slnx`
- Run tests: `dotnet run --project BeyondCrud.Domain.Tests` (TUnit on Microsoft.Testing.Platform; the test project is an `Exe`)
- Single test: `dotnet run --project BeyondCrud.Domain.Tests -- --treenode-filter "/*/*/PassengerTests/Register_emits_passenger_registered_event"`

## Architecture

- `BeyondCrud.AppHost` – Aspire orchestrator; wires `webfrontend` to `apiservice` (reference + wait-for, `/health` checks).
- `BeyondCrud.ServiceDefaults` – shared Aspire defaults (telemetry, health, resilience), applied via `AddServiceDefaults()` / `MapDefaultEndpoints()`.
- `BeyondCrud.ApiService` – minimal API; currently has a `/demo` endpoint that exercises the domain.
- `BeyondCrud.Web` – Blazor Server frontend.
- `BeyondCrud.Domain` – dependency-free domain library. Aggregates (e.g. `Passenger`) expose behavior methods that append events to an internal list, read via `GetUncommittedEvents()`. Commands live in `Commands/` (records), events in `Events/` (records implementing `IDomainEvent`). No persistence or event-store yet.
- `BeyondCrud.Domain.Tests` – tests assert on emitted events (Arrange/Act/Assert, async TUnit assertions: `await Assert.That(...)`).

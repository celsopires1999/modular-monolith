# AGENTS.md — Hotel Reservation Modular Monolith

## Setup
- `docker compose up -d` at the repo root starts the app + postgres (5432) + mongodb (27017) + mongo-express (8082)
- The dev container has dotnet-ef pre-installed and PATH configured
- The API runs at `http://localhost:5077`, Swagger at `/swagger`

## Working directory
- All `dotnet` commands must be run from: `FC4.HotelReservation/modular monolith/` (space in the name)
- Use quotes or escape: `cd FC4.HotelReservation/modular\ monolith`

## Database
- **PostgreSQL** (write, EF Core) — connection string: `Host=postgres;...` (works only inside the Docker network)
- **MongoDB** (read) — collections: `inventory`, `reservations` in `hotel_reservation` DB

## EF Core Migrations
- Migrations project: `shared/FC4.HotelReservation.Shared.Infrastructure`
- Startup project: `src/FC4.HotelReservation.WebApi`
- DbContext: `HotelDbContext`
- Command:
  ```
  dotnet ef migrations add <Name> --project "shared/FC4.HotelReservation.Shared.Infrastructure" --startup-project "src/FC4.HotelReservation.WebApi" --context HotelDbContext
  ```

## Seed data
- `./update-db.sh` inside the container runs migrations + seed (PostgreSQL only via EF Core)
- **SeedTool** (`tools/FC4.HotelReservation.SeedTool`) — CLI that seeds **PostgreSQL + MongoDB** and clears existing data
  - `--postgres` → only PostgreSQL | `--mongodb` → only MongoDB | `--all` (default) → both
  - Data: 1 hotel, 1 room type, 1 guest (GUID `11111111-...`), 60 days of inventory/rates
  - Dynamic start date (`DateTime.UtcNow.Date`)
  - Run: `dotnet run --project tools/FC4.HotelReservation.SeedTool -- --all`
- Manual testing uses fixed GUIDs (see `manual-testing/api.http`)

## Commands
- `dotnet build src/FC4.HotelReservation.WebApi` — build the API
- `dotnet test tests/FC4.HotelReservation.IntegrationTests` — integration tests (uses Testcontainers, no external dependencies)

## Architecture notes
- **CQRS**: writes → PostgreSQL via EF Core + UnitOfWork; reads → MongoDB via driver
- **Messaging**: MassTransit with PostgreSQL transport (outbox pattern)
- **MediatR**: handlers in `modules/*/Application/UseCases/[Entity]/[Action]/`
- **Minimal APIs**: endpoints in `src/FC4.HotelReservation.WebApi/Endpoints/`
- **4 modules**: Catalog, Guests, Payments, Reservations — each with Domain/Application/Infra.Data
- **Shared layer**: Shared.Domain (Entity, AggregateRoot, DomainEvent), Shared.Application (IUnitOfWork, exceptions), Shared.Infrastructure (EF Core DbContext, UnitOfWork, migrations)
- **Versioning**: `IVersioned` for optimistic concurrency on entities like RoomTypeInventory

## HTTP errors
| Exception | Status |
|---|---|
| ArgumentException | 400 |
| NotFoundException | 404 |
| ConflictException | 409 |
| InvalidOperationException | 422 |
| other | 500 |

## Testing
- xUnit `[Collection(nameof(WebApiFixture))]` pattern
- WebApiFixture inherits from `WebApplicationFactory<Program>` + Testcontainer PostgreSQL
- `IAsyncDisposable` + `CleanDatabaseAsync()` for cleanup between tests
- Bogus for fake data, FluentAssertions for assertions
- Concurrency tests use .NET `Barrier`

## Style conventions
- Nullable reference types and ImplicitUsings enabled (SDK defaults)
- Ardalis.GuardClauses for parameter validation
- Immutable records for commands/queries
- No `.editorconfig` — no enforced formatting rules
- Use English for all code, identifiers, comments, commit messages, and documentation

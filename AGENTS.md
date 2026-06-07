# AGENTS.md — Hotel Reservation Modular Monolith

## Setup
- `docker compose up -d` na raiz do repo inicia app + postgres (5432) + mongodb (27017) + mongo-express (8082)
- O container dev já tem dotnet-ef instalado e PATH configurado
- A API roda em `http://localhost:5077`, Swagger em `/swagger`

## Working directory
- Todos os comandos `dotnet` devem ser executados de: `FC4.HotelReservation/modular monolith/` (espaço no nome)
- Use aspas ou escape: `cd FC4.HotelReservation/modular\ monolith`

## Database
- **PostgreSQL** (escrita, EF Core) — connection string: `Host=postgres;...` (funciona apenas dentro da Docker network)
- **MongoDB** (leitura) — collections: `inventory`, `reservations` em `hotel_reservation` DB

## EF Core Migrations
- Projeto de migrations: `shared/FC4.HotelReservation.Shared.Infrastructure`
- Startup: `src/FC4.HotelReservation.WebApi`
- DbContext: `HotelDbContext`
- Comando:
  ```
  dotnet ef migrations add <Name> --project "shared/FC4.HotelReservation.Shared.Infrastructure" --startup-project "src/FC4.HotelReservation.WebApi" --context HotelDbContext
  ```

## Seed data
- `./update-db.sh` dentro do container aplica migrations + seed (apenas PostgreSQL via EF Core)
- **SeedTool** (`tools/FC4.HotelReservation.SeedTool`) — CLI que seeda **PostgreSQL + MongoDB** e limpa dados existentes
  - `--postgres` → só PostgreSQL | `--mongodb` → só MongoDB | `--all` (default) → ambos
  - Dados: 1 hotel, 1 room type, 1 guest (GUID `11111111-...`), 60 dias de inventory/rates
  - Data de início dinâmica (`DateTime.UtcNow.Date`)
  - Execução: `dotnet run --project tools/FC4.HotelReservation.SeedTool -- --all`
- Teste manual usa GUIDs fixos (veja `manual-testing/api.http`)

## Commands
- `dotnet build src/FC4.HotelReservation.WebApi` — build da API
- `dotnet test tests/FC4.HotelReservation.IntegrationTests` — testes de integração (usa Testcontainers, sem dependência externa)

## Architecture notes
- **CQRS**: writes → PostgreSQL via EF Core + UnitOfWork; reads → MongoDB via driver
- **Messaging**: MassTransit with PostgreSQL transport (outbox pattern)
- **MediatR**: handlers em `modules/*/Application/UseCases/[Entity]/[Action]/`
- **Minimal APIs**: endpoints em `src/FC4.HotelReservation.WebApi/Endpoints/`
- **4 módulos**: Catalog, Guests, Payments, Reservations — cada um com Domain/Application/Infra.Data
- **Shared layer**: Shared.Domain (Entity, AggregateRoot, DomainEvent), Shared.Application (IUnitOfWork, exceptions), Shared.Infrastructure (EF Core DbContext, UnitOfWork, migrations)
- **Versioning**: `IVersioned` para concorrência otimista em entidades como RoomTypeInventory

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
- WebApiFixture herda de `WebApplicationFactory<Program>` + Testcontainer PostgreSQL
- `IAsyncDisposable` + `CleanDatabaseAsync()` para limpeza entre testes
- Bogus para dados falsos, FluentAssertions para asserts
- Testes de concorrência usam `Barrier` do .NET

## Style conventions
- Nullable reference types e ImplicitUsings habilitados (padrão do SDK)
- Ardalis.GuardClauses para validação de parâmetros
- Registros imutáveis para comandos/queries (records)
- Nenhum `.editorconfig` — sem regras de formatação impostas

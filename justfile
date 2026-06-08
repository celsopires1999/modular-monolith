project_dir := "FC4.HotelReservation/modular monolith"
migrations_project := project_dir / "shared/FC4.HotelReservation.Shared.Infrastructure"
startup_project := project_dir / "src/FC4.HotelReservation.WebApi"
seed_project := project_dir / "tools/FC4.HotelReservation.SeedTool"
db_context := "HotelDbContext"

default:
    @just --list

# Build the WebApi project
build:
    dotnet build "{{project_dir}}/src/FC4.HotelReservation.WebApi"

# Run the API (Swagger at /swagger — http://localhost:5077)
api: build
    dotnet run --project "{{project_dir}}/src/FC4.HotelReservation.WebApi"

# Create a new migration (usage: just add-migration "MigrationName")
add-migration name:
    dotnet ef migrations add {{name}} --project "{{migrations_project}}" --startup-project "{{startup_project}}" --context {{db_context}}

# Apply all pending migrations
migrate:
    dotnet ef database update --project "{{migrations_project}}" --startup-project "{{startup_project}}" --context {{db_context}}

# Revert all migrations (back to empty state)
rollback:
    dotnet ef database update 0 --project "{{migrations_project}}" --startup-project "{{startup_project}}" --context {{db_context}}

# Revert to a specific migration (usage: just rollback-to "MigrationName")
rollback-to name:
    dotnet ef database update {{name}} --project "{{migrations_project}}" --startup-project "{{startup_project}}" --context {{db_context}}

# Remove the last migration
remove-migration:
    dotnet ef migrations remove --project "{{migrations_project}}" --startup-project "{{startup_project}}" --context {{db_context}}

# List all migrations
list-migrations:
    dotnet ef migrations list --project "{{migrations_project}}" --startup-project "{{startup_project}}" --context {{db_context}}

# Seed PostgreSQL + MongoDB (default: --all)
seed:
    dotnet run --project "{{seed_project}}" -- --all

# Seed only PostgreSQL
seed-pg:
    dotnet run --project "{{seed_project}}" -- --postgres

# Seed only MongoDB
seed-mongo:
    dotnet run --project "{{seed_project}}" -- --mongodb

# Run integration tests
test:
    dotnet test "{{project_dir}}/tests/FC4.HotelReservation.IntegrationTests"

# Apply migrations and seed data
setup: migrate seed

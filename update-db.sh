#!/bin/bash

cd /home/developer/app/FC4.HotelReservation/modular\ monolith

export ASPNETCORE_ENVIRONMENT=Development

# dotnet ef migrations add <MigrationName> --project "shared/FC4.HotelReservation.Shared.Infrastructure" --startup-project "src/FC4.HotelReservation.WebApi" --context HotelDbContext

dotnet ef database update --project "shared/FC4.HotelReservation.Shared.Infrastructure" --startup-project "src/FC4.HotelReservation.WebApi" --context HotelDbContext

# dotnet ef database update 0 --project "shared/FC4.HotelReservation.Shared.Infrastructure" --startup-project "src/FC4.HotelReservation.WebApi" --context HotelDbContext
dotnet ef migrations add RowVersion --project "shared/FC4.HotelReservation.Shared.Infrastructure" --startup-project "src/FC4.HotelReservation.WebApi"

Executando o comando... resultado:

developer ➜ ~/app/FC4.HotelReservation/modular monolith (main) $ dotnet ef migrations add RowVersion --project "shared/FC4.HotelReservation.Shared.Infrastructure" --startup-project "src/FC4.HotelReservation.WebApi"
Build started...
Build succeeded.
warn: Microsoft.EntityFrameworkCore.Model.Validation[10400]
      Sensitive data logging is enabled. Log entries and exception messages may include sensitive application data; this mode should only be enabled during development.
warn: 5/31/2026 00:05:40.185 CoreEventId.SensitiveDataLoggingEnabledWarning[10400] (Microsoft.EntityFrameworkCore.Infrastructure) 
      Sensitive data logging is enabled. Log entries and exception messages may include sensitive application data; this mode should only be enabled during development.
Done. To undo this action, use 'ef migrations remove'
developer ➜ ~/app/FC4.HotelReservation/modular monolith (main) $ 
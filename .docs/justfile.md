### Starting the Application

cd "/home/developer/app"
./update-db.sh

cd "/home/developer/app/FC4.HotelReservation/modular monolith"
dotnet run --project tools/FC4.HotelReservation.SeedTool -- --all

cd "/home/developer/app/FC4.HotelReservation/modular monolith"
dotnet run --project src/FC4.HotelReservation.WebApi

cd "/home/developer/app/FC4.HotelReservation/modular monolith"
dotnet test
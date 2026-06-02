using FC4.HotelReservation.Reservations.Application.Queries.Common;
using FC4.HotelReservation.Reservations.Domain.Repositories;
using FC4.HotelReservation.Reservations.Infra.Data.DAOs;
using FC4.HotelReservation.Reservations.Infra.Data.Repositories;
using FC4.HotelReservation.Shared.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Data;

namespace FC4.HotelReservation.Reservations.Infra.Data;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddReservationsRepositories(this IServiceCollection services)
    {
        return services
            .AddScoped<IReservationRepository, ReservationRepository>()
            .AddScoped<IReservationGuestRepository, ReservationGuestRepository>()
            .AddScoped<IRoomTypeInventoryRepository, RoomTypeInventoryRepository>()
            .AddScoped<IReservationDao, ReservationDao>()
            // TODO: This is a workaround to allow using Dapper with the same DbContext. We should consider a better approach in the future, like using a separate connection or a micro-ORM that can work with EF Core's DbContext.
            .AddScoped<IDbConnection>(sp => sp.GetRequiredService<HotelDbContext>().Database.GetDbConnection());
    }
}
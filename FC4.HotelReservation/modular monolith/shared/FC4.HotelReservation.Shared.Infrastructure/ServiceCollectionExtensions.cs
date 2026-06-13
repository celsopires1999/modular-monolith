using FC4.HotelReservation.Reservations.Domain.Entities;
using FC4.HotelReservation.Shared.Application;
using FC4.HotelReservation.Shared.Infrastructure.EventStore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FC4.HotelReservation.Shared.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        return services
            .AddScoped<IUnitOfWork, UnitOfWork>()
            // .AddScoped(typeof(EventStoreRepository<>)) // This line is commented to remember that is is also an option to register the generic repository without specifying the type parameter, allowing it to be injected for any entity type.
            .AddScoped<EventStoreRepository<Reservation>>()
            .AddScoped<EventStoreRepository<RoomTypeInventory>>()
            .AddDbContext<HotelDbContext>((serviceProvider, options) =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();

                options.UseNpgsql(configuration.GetConnectionString("HotelReservationDb"));

                var efCoreLogLevel = configuration["Logging:LogLevel:EfCore"] ?? "Warning";
                if (Enum.TryParse<LogLevel>(efCoreLogLevel, ignoreCase: true, out var logLevel)
                    && logLevel <= LogLevel.Information)
                {
                    options.EnableSensitiveDataLogging();
                    options.LogTo(Console.WriteLine, logLevel);
                }
            });
    }
}
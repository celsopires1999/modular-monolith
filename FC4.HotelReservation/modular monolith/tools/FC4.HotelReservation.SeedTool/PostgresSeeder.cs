using FC4.HotelReservation.Shared.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FC4.HotelReservation.SeedTool;

public class PostgresSeeder(HotelDbContext context)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await context.Database.EnsureCreatedAsync(cancellationToken);
        await ClearDataAsync(cancellationToken);
        await SeedDataAsync(cancellationToken);
    }

    private async Task ClearDataAsync(CancellationToken cancellationToken)
    {
        await context.Database.ExecuteSqlRawAsync("""
            DELETE FROM room_type_rates;
            DELETE FROM room_type_inventory;
            DELETE FROM rooms;
            DELETE FROM reservations;
            DELETE FROM payments;
            DELETE FROM guests;
            DELETE FROM room_types;
            DELETE FROM hotels;
            """, cancellationToken);
    }

    private async Task SeedDataAsync(CancellationToken cancellationToken)
    {
        var hotelId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var roomTypeId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var guestId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var startDate = DateTime.UtcNow.Date;

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        await context.Database.ExecuteSqlRawAsync("""
            INSERT INTO hotels (id, name, street, city, state, country, zip_code)
            VALUES ({0}, 'Grand Hotel Plaza', '123 Main Street', 'New York', 'NY', 'USA', '10001');
            """, [hotelId], cancellationToken);

        await context.Database.ExecuteSqlRawAsync("""
            INSERT INTO room_types (id, description)
            VALUES ({0}, 'Standard Room');
            """, [roomTypeId], cancellationToken);

        await context.Database.ExecuteSqlRawAsync("""
            INSERT INTO guests (id, first_name, last_name, email)
            VALUES ({0}, 'John', 'Doe', 'john.doe@example.com');
            """, [guestId], cancellationToken);

        for (int dayOffset = 0; dayOffset < 60; dayOffset++)
        {
            var currentDate = startDate.AddDays(dayOffset);

            var inventoryId = new Guid($"44444444-4444-4444-4444-{(dayOffset + 1):D12}");
            var rateId = new Guid($"11111111-1111-1111-1111-{(dayOffset + 1):D12}");

            var rateAmount = 150.00m;
            if (currentDate.DayOfWeek is DayOfWeek.Friday or DayOfWeek.Saturday)
            {
                rateAmount *= 1.3m;
            }

            await context.Database.ExecuteSqlRawAsync("""
                INSERT INTO room_type_inventory (id, room_type_id, hotel_id, date, total_inventory, total_reserved, version)
                VALUES ({0}, {1}, {2}, {3}, 10, 0, 1);
                """, [inventoryId, roomTypeId, hotelId, currentDate], cancellationToken);

            await context.Database.ExecuteSqlRawAsync("""
                INSERT INTO room_type_rates (id, room_type_id, hotel_id, date, rate_amount, rate_currency)
                VALUES ({0}, {1}, {2}, {3}, {4}, {5});
                """, [rateId, roomTypeId, hotelId, currentDate, rateAmount, "USD"], cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }
}

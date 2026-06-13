using System.Text.Json;
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
            DELETE FROM room_type_inventory_projections;
            DELETE FROM event_store;
            DELETE FROM "OutboxMessage";
            DELETE FROM "InboxState";
            DELETE FROM "OutboxState";
            DELETE FROM rooms;
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
        var occurredOn = DateTime.UtcNow;

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
            var eventId = new Guid($"55555555-5555-5555-5555-{(dayOffset + 1):D12}");

            var rateAmount = 150.00m;
            if (currentDate.DayOfWeek is DayOfWeek.Friday or DayOfWeek.Saturday)
            {
                rateAmount *= 1.3m;
            }

            await context.Database.ExecuteSqlRawAsync("""
                INSERT INTO room_type_inventory_projections (id, hotel_id, room_type_id, date)
                VALUES ({0}, {1}, {2}, {3});
                """, [inventoryId, hotelId, roomTypeId, currentDate], cancellationToken);

            var eventData = JsonSerializer.Serialize(new
            {
                InventoryId = inventoryId,
                HotelId = hotelId,
                RoomTypeId = roomTypeId,
                Date = currentDate,
                TotalInventory = 10,
                EventId = eventId,
                AggregateId = inventoryId,
                AggregateVersion = 0,
                OccuredOn = occurredOn
            });

            await context.Database.ExecuteSqlRawAsync("""
                INSERT INTO event_store (event_id, aggregate_id, aggregate_version, event_data, event_type, occurred_on)
                VALUES ({0}, {1}, 0, {2}, 'RoomTypeInventoryCreatedEvent', {3});
                """, [eventId, inventoryId, eventData, occurredOn], cancellationToken);

            await context.Database.ExecuteSqlRawAsync("""
                INSERT INTO room_type_rates (id, room_type_id, hotel_id, date, rate_amount, rate_currency)
                VALUES ({0}, {1}, {2}, {3}, {4}, {5});
                """, [rateId, roomTypeId, hotelId, currentDate, rateAmount, "USD"], cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }
}

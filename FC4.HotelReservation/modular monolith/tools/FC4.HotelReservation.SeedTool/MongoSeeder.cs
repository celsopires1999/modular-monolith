using FC4.HotelReservation.Reservations.Consumers.Models;
using MongoDB.Driver;

namespace FC4.HotelReservation.SeedTool;

public class MongoSeeder(IMongoDatabase database)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await CleanCollectionsAsync(cancellationToken);
        await SeedInventoryAsync(cancellationToken);
    }

    private async Task CleanCollectionsAsync(CancellationToken cancellationToken)
    {
        var inventoryCollection = database.GetCollection<InventoryModel>("inventory");
        await inventoryCollection.DeleteManyAsync(FilterDefinition<InventoryModel>.Empty, cancellationToken);

        var reservationCollection = database.GetCollection<ReservationModel>("reservations");
        await reservationCollection.DeleteManyAsync(FilterDefinition<ReservationModel>.Empty, cancellationToken);
    }

    private async Task SeedInventoryAsync(CancellationToken cancellationToken)
    {
        var collection = database.GetCollection<InventoryModel>("inventory");

        var hotelId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var roomTypeId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var startDate = DateTime.UtcNow.Date;

        var inventories = new List<InventoryModel>(60);

        for (int dayOffset = 0; dayOffset < 60; dayOffset++)
        {
            inventories.Add(new InventoryModel
            {
                InventoryId = new Guid($"44444444-4444-4444-4444-{(dayOffset + 1):D12}"),
                HotelId = hotelId,
                RoomTypeId = roomTypeId,
                Date = startDate.AddDays(dayOffset),
                Quantity = 10,
                UpdatedAt = DateTime.UtcNow
            });
        }

        await collection.InsertManyAsync(inventories, cancellationToken: cancellationToken);
    }
}

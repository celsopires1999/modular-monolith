using MongoDB.Bson;
using MongoDB.Driver;

namespace FC4.HotelReservation.SeedTool;

public class MongoSeeder(IMongoDatabase database)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var inventoryCollection = database.GetCollection<BsonDocument>("inventory");
        var reservationCollection = database.GetCollection<BsonDocument>("reservations");

        await inventoryCollection.DeleteManyAsync(FilterDefinition<BsonDocument>.Empty, cancellationToken);
        await reservationCollection.DeleteManyAsync(FilterDefinition<BsonDocument>.Empty, cancellationToken);

        await RebuildIndexesAsync(inventoryCollection, cancellationToken);

        var hotelId = "11111111-1111-1111-1111-111111111111";
        var roomTypeId = "11111111-1111-1111-1111-111111111111";
        var startDate = DateTime.UtcNow.Date;

        var inventories = new List<BsonDocument>(60);

        for (int dayOffset = 0; dayOffset < 60; dayOffset++)
        {
            inventories.Add(new BsonDocument
            {
                ["_id"] = $"44444444-4444-4444-4444-{(dayOffset + 1):D12}",
                ["HotelId"] = hotelId,
                ["RoomTypeId"] = roomTypeId,
                ["Date"] = startDate.AddDays(dayOffset),
                ["Quantity"] = 10,
                ["UpdatedAt"] = DateTime.UtcNow
            });
        }

        await inventoryCollection.InsertManyAsync(inventories, cancellationToken: cancellationToken);
    }

    private async Task RebuildIndexesAsync(IMongoCollection<BsonDocument> collection, CancellationToken cancellationToken)
    {
        var existingIndexes = await (await collection.Indexes.ListAsync(cancellationToken)).ToListAsync(cancellationToken);

        foreach (var index in existingIndexes)
        {
            var key = index["key"].AsBsonDocument;
            if (key.Names.Any(n => n is "hotelId" or "roomTypeId" or "date"))
            {
                var name = index["name"].AsString;
                await collection.Indexes.DropOneAsync(name, cancellationToken);
            }
        }

        var indexKeys = Builders<BsonDocument>.IndexKeys
            .Ascending("HotelId")
            .Ascending("RoomTypeId")
            .Ascending("Date");

        await collection.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                indexKeys,
                new CreateIndexOptions { Name = "idx_inventory_hotel_roomtype_date", Unique = true }),
            cancellationToken: cancellationToken);
    }
}

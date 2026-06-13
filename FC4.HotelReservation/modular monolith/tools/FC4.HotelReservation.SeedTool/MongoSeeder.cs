using FC4.HotelReservation.Reservations.Consumers.Models;
using FC4.HotelReservation.Shared.Infrastructure;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace FC4.HotelReservation.SeedTool;

public class MongoSeeder(HotelDbContext context, IMongoDatabase database)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var inventoryCollection = database.GetCollection<InventoryModel>("inventory");
        var reservationCollection = database.GetCollection<InventoryModel>("reservations");

        await inventoryCollection.DeleteManyAsync(FilterDefinition<InventoryModel>.Empty, cancellationToken);
        await reservationCollection.DeleteManyAsync(FilterDefinition<InventoryModel>.Empty, cancellationToken);

        await RebuildIndexesAsync(inventoryCollection, cancellationToken);

        var projections = await context.RoomTypeInventoryProjections
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var inventories = projections.Select(p => new InventoryModel
        {
            InventoryId = p.Id,
            HotelId = p.HotelId,
            RoomTypeId = p.RoomTypeId,
            Date = p.Date,
            Quantity = 10,
            UpdatedAt = DateTime.UtcNow
        }).ToList();

        if (inventories.Count > 0)
        {
            await inventoryCollection.InsertManyAsync(inventories, cancellationToken: cancellationToken);
        }
    }

    private async Task RebuildIndexesAsync(IMongoCollection<InventoryModel> collection, CancellationToken cancellationToken)
    {
        var existingIndexes = await (await collection.Indexes.ListAsync(cancellationToken)).ToListAsync(cancellationToken);

        foreach (var index in existingIndexes)
        {
            var key = index["key"].AsBsonDocument;
            if (key.Names.Any(n => n is "HotelId" or "RoomTypeId" or "Date"))
            {
                var name = index["name"].AsString;
                await collection.Indexes.DropOneAsync(name, cancellationToken);
            }
        }

        var indexKeys = Builders<InventoryModel>.IndexKeys
            .Ascending(i => i.HotelId)
            .Ascending(i => i.RoomTypeId)
            .Ascending(i => i.Date);

        await collection.Indexes.CreateOneAsync(
            new CreateIndexModel<InventoryModel>(
                indexKeys,
                new CreateIndexOptions { Name = "idx_inventory_hotel_roomtype_date", Unique = true }),
            cancellationToken: cancellationToken);
    }
}

using FC4.HotelReservation.SeedTool;
using FC4.HotelReservation.Shared.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;

var mode = args.Length > 0 ? args[0].TrimStart('-') : "all";

if (mode is not ("postgres" or "mongodb" or "all"))
{
    Console.Error.WriteLine("Usage: --postgres | --mongodb | --all (default)");
    return 1;
}

var assemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
var contentRoot = Path.GetDirectoryName(assemblyPath)!;

var host = Host.CreateDefaultBuilder(args)
    .UseContentRoot(contentRoot)
    .ConfigureAppConfiguration(config =>
    {
        config.AddJsonFile("appsettings.json", optional: false);
    })
    .ConfigureServices((context, services) =>
    {
        var postgresConnectionString =
            context.Configuration.GetConnectionString("HotelReservationDb")!;

        services.AddDbContext<HotelDbContext>(options =>
            options.UseNpgsql(postgresConnectionString));

        var mongoSettings = context.Configuration.GetSection("MongoDbSettings");
        var mongoConnectionString = mongoSettings["ConnectionString"]!;
        var mongoDatabaseName = mongoSettings["DatabaseName"]!;

        services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoConnectionString));
        services.AddSingleton(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            return client.GetDatabase(mongoDatabaseName);
        });
    })
    .Build();

if (mode is "postgres" or "all")
{
    Console.WriteLine("Seeding PostgreSQL...");
    using var scope = host.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<HotelDbContext>();
    var postgresSeeder = new PostgresSeeder(dbContext);
    await postgresSeeder.SeedAsync();
    Console.WriteLine("PostgreSQL seeded successfully.");
}

if (mode is "mongodb" or "all")
{
    Console.WriteLine("Seeding MongoDB...");
    using var scope = host.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<HotelDbContext>();
    var database = host.Services.GetRequiredService<IMongoDatabase>();
    var mongoSeeder = new MongoSeeder(dbContext, database);
    await mongoSeeder.SeedAsync();
    Console.WriteLine("MongoDB seeded successfully.");
}

Console.WriteLine("Seed completed.");
return 0;

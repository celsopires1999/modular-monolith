using System.Text.Json;
using FC4.HotelReservation.Shared.Infrastructure;
using FC4.HotelReservation.WebApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Testcontainers.MongoDb;
using Testcontainers.PostgreSql;

namespace FC4.HotelReservation.IntegrationTests;

public partial class WebApiFixture : WebApplicationFactory<Program>
{
    public PostgreSqlContainer PostgresDb { get; } =
        new PostgreSqlBuilder()
            .WithDatabase("hotel_reservation")
            .WithImage("postgres:16")
            .Build();

    public MongoDbContainer MongoDb { get; } =
        new MongoDbBuilder()
            .WithImage("mongo:7")
            .Build();

    public JsonSerializerOptions JsonSettings { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("IntegrationTests");
        
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string>
            {
                { "ConnectionStrings:HotelReservationDb", PostgresDb.GetConnectionString() },
                { "MongoDb:ConnectionString", MongoDb.GetConnectionString() },
            }!);
        });

        // Register MigrationHostedService to run EF Core migrations BEFORE
        // MassTransit bus starts, ensuring OutboxMessage/InboxState/OutboxState
        // tables exist when the BusOutboxDeliverer begins.
        builder.ConfigureServices(services =>
        {
            services.Insert(0, ServiceDescriptor.Transient<IHostedService, MigrationHostedService>());
        });
    }
    
    protected override IHost CreateHost(IHostBuilder builder)
    {
        PostgresDb.StartAsync().GetAwaiter().GetResult();
        MongoDb.StartAsync().GetAwaiter().GetResult();

        return base.CreateHost(builder);
    }
    
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            PostgresDb.DisposeAsync().AsTask().Wait();
            MongoDb.DisposeAsync().AsTask().Wait();
        }
        base.Dispose(disposing);
    }
}

[CollectionDefinition(nameof(WebApiFixture))]
public class CustomWebApplicationFactoryCollectionFixture : ICollectionFixture<WebApiFixture>;
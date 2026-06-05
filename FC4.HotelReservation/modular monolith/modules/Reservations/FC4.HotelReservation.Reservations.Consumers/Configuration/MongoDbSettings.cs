namespace FC4.HotelReservation.Reservations.Consumers.Configuration;

public class MongoDbSettings
{
    public string ConnectionString { get; set; } = "mongodb://mongodb:27017";
    public string DatabaseName { get; set; } = "hotel_reservation";
}
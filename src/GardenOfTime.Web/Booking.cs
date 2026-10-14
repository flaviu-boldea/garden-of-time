namespace GardenOfTime.Web;

public record BookingRequest(string Name, string Email, string? Phone, DateOnly Date, string EventType, int Guests, string? Notes);

public record Booking(string Id, string Name, string Email, string? Phone, DateOnly Date, string EventType, int Guests, string? Notes, DateTimeOffset CreatedAt);

public class BookingException(string message) : Exception(message);

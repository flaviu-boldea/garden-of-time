using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace GardenOfTime.Web;

public class GardenInfoPlugin(IConfiguration config)
{
    [KernelFunction, Description("Returns information about the Garden of Time (Grădina Timpului) in La Boldici: description, events, hours, contact.")]
    public string GetGardenInfo() =>
        config["Garden:Info"] ?? "Grădina Timpului (Garden of Time) is a garden in La Boldici hosting events. Details: https://sites.google.com/view/gradinatimpului-laboldici/acasa";

    [KernelFunction, Description("Returns information about the surrounding area: attractions, accommodation, directions.")]
    public string GetAreaInfo() =>
        config["Garden:AreaInfo"] ?? "No area information configured yet. Ask the garden owner.";
}

public class BookingPlugin(BookingService bookings)
{
    [KernelFunction, Description("Checks whether the garden is free on a date (yyyy-MM-dd).")]
    public async Task<string> CheckAvailability(string date) =>
        !DateOnly.TryParse(date, out var d) ? "Invalid date."
        : await bookings.IsAvailableAsync(d) ? "Available" : "Not available";

    [KernelFunction, Description("Creates an event booking once the customer has provided name, email, date (yyyy-MM-dd), event type and number of guests.")]
    public async Task<string> CreateBooking(string name, string email, string date, string eventType, int guests, string? phone = null, string? notes = null)
    {
        if (!DateOnly.TryParse(date, out var d)) return "Invalid date.";
        try
        {
            var b = await bookings.CreateAsync(new BookingRequest(name, email, phone, d, eventType, guests, notes));
            return $"Booking confirmed. Reference: {b.Id}";
        }
        catch (BookingException e)
        {
            return $"Booking failed: {e.Message}";
        }
    }
}

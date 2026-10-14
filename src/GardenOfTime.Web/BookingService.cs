using System.Net.Mail;

namespace GardenOfTime.Web;

public class BookingService(IBookingStore store, TimeProvider time)
{
    public const int MaxGuests = 150;

    public async Task<Booking> CreateAsync(BookingRequest r)
    {
        if (string.IsNullOrWhiteSpace(r.Name)) throw new BookingException("Name is required.");
        if (!IsValidEmail(r.Email)) throw new BookingException("A valid email is required.");
        if (string.IsNullOrWhiteSpace(r.EventType)) throw new BookingException("Event type is required.");
        if (r.Guests < 1 || r.Guests > MaxGuests) throw new BookingException($"Guests must be between 1 and {MaxGuests}.");
        if (r.Date <= DateOnly.FromDateTime(time.GetUtcNow().UtcDateTime)) throw new BookingException("Date must be in the future.");

        var booking = new Booking(Guid.NewGuid().ToString("N"), r.Name.Trim(), r.Email.Trim(), r.Phone?.Trim(),
            r.Date, r.EventType.Trim(), r.Guests, r.Notes?.Trim(), time.GetUtcNow());
        if (!await store.TryAddAsync(booking)) throw new BookingException($"{r.Date:yyyy-MM-dd} is already booked.");
        return booking;
    }

    public Task<bool> IsAvailableAsync(DateOnly date) => store.IsDateTakenAsync(date).ContinueWith(t => !t.Result);

    private static bool IsValidEmail(string? e) =>
        !string.IsNullOrWhiteSpace(e) && MailAddress.TryCreate(e.Trim(), out var a) && a.Address == e.Trim();
}

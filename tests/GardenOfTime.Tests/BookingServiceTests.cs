using GardenOfTime.Web;

namespace GardenOfTime.Tests;

public class BookingServiceTests
{
    private static BookingService Create() => new(new InMemoryBookingStore(), TimeProvider.System);
    private static BookingRequest Req(DateOnly d, string email = "a@b.com", int guests = 20) => new("Ana", email, null, d, "Wedding", guests, null);
    private static DateOnly Future => DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30));

    [Fact]
    public async Task Creates_booking_and_blocks_date()
    {
        var s = Create();
        await s.CreateAsync(Req(Future));
        Assert.False(await s.IsAvailableAsync(Future));
        var unavailable = await s.GetUnavailableDatesAsync(Future.AddDays(-1), Future.AddDays(1));
        Assert.Contains(Future, unavailable);
        Assert.Single(unavailable);
        await Assert.ThrowsAsync<BookingException>(() => s.CreateAsync(Req(Future)));
    }

    [Theory]
    [InlineData("bad", 10)]
    [InlineData("a@b.com", 0)]
    [InlineData("a@b.com", 1000)]
    public async Task Rejects_invalid(string email, int guests) =>
        await Assert.ThrowsAsync<BookingException>(() => Create().CreateAsync(Req(Future, email, guests)));

    [Fact]
    public async Task Rejects_past_date() =>
        await Assert.ThrowsAsync<BookingException>(() => Create().CreateAsync(Req(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)))));
}

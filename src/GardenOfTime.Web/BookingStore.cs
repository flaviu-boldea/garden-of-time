using System.Collections.Concurrent;
using Azure.Data.Tables;

namespace GardenOfTime.Web;

public interface IBookingStore
{
    Task<bool> TryAddAsync(Booking booking);
    Task<bool> IsDateTakenAsync(DateOnly date);
}

public class InMemoryBookingStore : IBookingStore
{
    private readonly ConcurrentDictionary<DateOnly, Booking> _byDate = new();

    public Task<bool> TryAddAsync(Booking booking) => Task.FromResult(_byDate.TryAdd(booking.Date, booking));

    public Task<bool> IsDateTakenAsync(DateOnly date) => Task.FromResult(_byDate.ContainsKey(date));
}

/// <summary>Azure Table Storage store: one booking per date (PartitionKey = year, RowKey = date), so a date can't be double booked.</summary>
public class TableBookingStore : IBookingStore
{
    private readonly TableClient _table;
    private bool _created;

    public TableBookingStore(TableClient table) => _table = table;

    private async Task EnsureAsync()
    {
        if (_created) return;
        await _table.CreateIfNotExistsAsync();
        _created = true;
    }

    public async Task<bool> TryAddAsync(Booking b)
    {
        await EnsureAsync();
        var entity = new TableEntity(b.Date.Year.ToString(), b.Date.ToString("yyyy-MM-dd"))
        {
            ["Id"] = b.Id, ["Name"] = b.Name, ["Email"] = b.Email, ["Phone"] = b.Phone,
            ["EventType"] = b.EventType, ["Guests"] = b.Guests, ["Notes"] = b.Notes,
            ["CreatedAt"] = b.CreatedAt,
        };
        try
        {
            await _table.AddEntityAsync(entity);
            return true;
        }
        catch (Azure.RequestFailedException e) when (e.Status == 409)
        {
            return false;
        }
    }

    public async Task<bool> IsDateTakenAsync(DateOnly date)
    {
        await EnsureAsync();
        var r = await _table.GetEntityIfExistsAsync<TableEntity>(date.Year.ToString(), date.ToString("yyyy-MM-dd"));
        return r.HasValue;
    }
}

using System.Collections.Concurrent;
using Azure.Data.Tables;

namespace GardenOfTime.Web;

public interface IBookingStore
{
    Task<bool> TryAddAsync(Booking booking);
    Task<bool> IsDateTakenAsync(DateOnly date);
    Task<IReadOnlyCollection<DateOnly>> GetTakenDatesAsync(DateOnly from, DateOnly through);
}

public class InMemoryBookingStore : IBookingStore
{
    private readonly ConcurrentDictionary<DateOnly, Booking> _byDate = new();

    public Task<bool> TryAddAsync(Booking booking) => Task.FromResult(_byDate.TryAdd(booking.Date, booking));

    public Task<bool> IsDateTakenAsync(DateOnly date) => Task.FromResult(_byDate.ContainsKey(date));

    public Task<IReadOnlyCollection<DateOnly>> GetTakenDatesAsync(DateOnly from, DateOnly through) =>
        Task.FromResult<IReadOnlyCollection<DateOnly>>(_byDate.Keys.Where(date => date >= from && date <= through).ToArray());
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

    public async Task<IReadOnlyCollection<DateOnly>> GetTakenDatesAsync(DateOnly from, DateOnly through)
    {
        await EnsureAsync();
        var dates = new List<DateOnly>();
        for (var year = from.Year; year <= through.Year; year++)
        {
            var yearStart = year == from.Year ? from : new DateOnly(year, 1, 1);
            var yearEnd = year == through.Year ? through : new DateOnly(year, 12, 31);
            var partitionKey = year.ToString();
            var firstRowKey = yearStart.ToString("yyyy-MM-dd");
            var lastRowKey = yearEnd.ToString("yyyy-MM-dd");
            var filter = TableClient.CreateQueryFilter(
                $"PartitionKey eq {partitionKey} and RowKey ge {firstRowKey} and RowKey le {lastRowKey}");
            await foreach (var entity in _table.QueryAsync<TableEntity>(filter))
                dates.Add(DateOnly.ParseExact(entity.RowKey, "yyyy-MM-dd"));
        }
        return dates;
    }
}

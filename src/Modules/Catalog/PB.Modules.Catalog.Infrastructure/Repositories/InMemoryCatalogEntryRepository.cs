namespace PB.Modules.Catalog.Infrastructure.Repositories;

using PB.Modules.Catalog.Domain.Entities;
using PB.Modules.Catalog.Domain.Repositories;
using System.Collections.Concurrent;

public class InMemoryCatalogEntryRepository : ICatalogEntryRepository
{
    private readonly ConcurrentDictionary<Guid, CatalogEntry> _store = new();

    public Task<CatalogEntry?> GetByIdAsync(Guid id)
    {
        _store.TryGetValue(id, out var entry);
        return Task.FromResult(entry);
    }

    public Task<IReadOnlyList<CatalogEntry>> GetAllAsync()
    {
        IReadOnlyList<CatalogEntry> result = _store.Values.ToList();
        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<CatalogEntry>> SearchAsync(string? city, DateOnly? from, DateOnly? to, string? category)
    {
        var query = _store.Values.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(city))
            query = query.Where(e => e.Location.City.Equals(city, StringComparison.OrdinalIgnoreCase));

        if (from.HasValue && to.HasValue)
            query = query.Where(e => e.IsAvailableBetween(from.Value, to.Value));

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(e => e.Category.Equals(category, StringComparison.OrdinalIgnoreCase));

        IReadOnlyList<CatalogEntry> result = query.ToList();
        return Task.FromResult(result);
    }

    public Task AddAsync(CatalogEntry entry)
    {
        _store[entry.Id] = entry;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        _store.TryRemove(id, out _);
        return Task.CompletedTask;
    }
}

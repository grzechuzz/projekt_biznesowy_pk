using System.Collections.Concurrent;
using PB.Modules.Catalog.Domain.Aggregates;
using PB.Modules.Catalog.Domain.Enums;
using PB.Modules.Catalog.Domain.Ports;

namespace PB.Modules.Catalog.Infrastructure.Repositories;

public class InMemoryCatalogEntryRepository : ICatalogEntryRepository
{
    private readonly ConcurrentDictionary<Guid, CatalogEntry> _store = new();

    public Task<CatalogEntry?> GetByIdAsync(Guid id)
    {
        _store.TryGetValue(id, out var result);
        return Task.FromResult(result);
    }

    public Task<IEnumerable<CatalogEntry>> GetAllAsync()
        => Task.FromResult<IEnumerable<CatalogEntry>>(_store.Values.ToList());

    public Task<IEnumerable<CatalogEntry>> SearchAsync(string? city, DateOnly? from, DateOnly? to, IEnumerable<string>? tags, CatalogEntryStatus? status)
    {
        var query = _store.Values.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(city))
            query = query.Where(e => e.Location.City.Contains(city, StringComparison.OrdinalIgnoreCase));

        if (from.HasValue)
            query = query.Where(e => e.DateRange.To >= from.Value);

        if (to.HasValue)
            query = query.Where(e => e.DateRange.From <= to.Value);

        if (tags != null)
        {
            var tagList = tags.ToList();
            if (tagList.Any())
                query = query.Where(e => e.Tags.Any(t => tagList.Any(tl => t.Name.Contains(tl, StringComparison.OrdinalIgnoreCase))));
        }

        if (status.HasValue)
            query = query.Where(e => e.Status == status.Value);

        return Task.FromResult<IEnumerable<CatalogEntry>>(query.ToList());
    }

    public Task<IEnumerable<CatalogEntry>> GetByAttractionDefinitionIdAsync(Guid attractionDefinitionId)
    {
        var results = _store.Values.Where(e => e.AttractionDefinitionId == attractionDefinitionId).ToList();
        return Task.FromResult<IEnumerable<CatalogEntry>>(results);
    }

    public Task AddAsync(CatalogEntry entry)
    {
        _store[entry.Id] = entry;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(CatalogEntry entry)
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

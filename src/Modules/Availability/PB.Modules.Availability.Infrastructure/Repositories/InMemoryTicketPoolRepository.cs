using System.Collections.Concurrent;
using PB.Modules.Availability.Domain.Aggregates;
using PB.Modules.Availability.Domain.Ports;

namespace PB.Modules.Availability.Infrastructure.Repositories;

public class InMemoryTicketPoolRepository : ITicketPoolRepository
{
    private readonly ConcurrentDictionary<Guid, TicketPool> _store = new();

    public Task<TicketPool?> GetByIdAsync(Guid id)
    {
        _store.TryGetValue(id, out var result);
        return Task.FromResult(result);
    }

    public Task<TicketPool?> GetByCatalogEntryIdAsync(Guid catalogEntryId)
    {
        var result = _store.Values.FirstOrDefault(p => p.CatalogEntryId == catalogEntryId);
        return Task.FromResult(result);
    }

    public Task<IEnumerable<TicketPool>> GetAllAsync()
        => Task.FromResult<IEnumerable<TicketPool>>(_store.Values.ToList());

    public Task AddAsync(TicketPool pool)
    {
        _store[pool.Id] = pool;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(TicketPool pool)
    {
        _store[pool.Id] = pool;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        _store.TryRemove(id, out _);
        return Task.CompletedTask;
    }
}

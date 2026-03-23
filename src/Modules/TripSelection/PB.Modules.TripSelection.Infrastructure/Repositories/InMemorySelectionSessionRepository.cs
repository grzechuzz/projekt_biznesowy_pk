using System.Collections.Concurrent;
using PB.Modules.TripSelection.Domain.Aggregates;
using PB.Modules.TripSelection.Domain.Ports;

namespace PB.Modules.TripSelection.Infrastructure.Repositories;

public class InMemorySelectionSessionRepository : ISelectionSessionRepository
{
    private readonly ConcurrentDictionary<Guid, SelectionSession> _store = new();

    public Task<SelectionSession?> GetByIdAsync(Guid id)
    {
        _store.TryGetValue(id, out var result);
        return Task.FromResult(result);
    }

    public Task AddAsync(SelectionSession session)
    {
        _store[session.Id] = session;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(SelectionSession session)
    {
        _store[session.Id] = session;
        return Task.CompletedTask;
    }
}

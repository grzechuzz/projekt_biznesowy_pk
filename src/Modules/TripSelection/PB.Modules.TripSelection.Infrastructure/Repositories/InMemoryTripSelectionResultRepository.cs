namespace PB.Modules.TripSelection.Infrastructure.Repositories;

using PB.Modules.TripSelection.Domain.Entities;
using PB.Modules.TripSelection.Domain.Repositories;
using System.Collections.Concurrent;

public class InMemoryTripSelectionResultRepository : ITripSelectionResultRepository
{
    private readonly ConcurrentDictionary<Guid, TripSelectionResult> _store = new();

    public Task<TripSelectionResult?> GetByIdAsync(Guid id)
    {
        _store.TryGetValue(id, out var result);
        return Task.FromResult(result);
    }

    public Task AddAsync(TripSelectionResult result)
    {
        _store[result.Id] = result;
        return Task.CompletedTask;
    }
}

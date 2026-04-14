using System.Collections.Concurrent;
using PB.Modules.TripSelection.Domain.Aggregates;
using PB.Modules.TripSelection.Domain.Ports;

namespace PB.Modules.TripSelection.Infrastructure.Repositories;

public class InMemoryAttractionRelationRepository : IAttractionRelationRepository
{
    private readonly ConcurrentDictionary<Guid, AttractionRelation> _store = new();

    public Task<AttractionRelation?> GetByIdAsync(Guid id)
    {
        _store.TryGetValue(id, out var result);
        return Task.FromResult(result);
    }

    public Task<IEnumerable<AttractionRelation>> GetAllAsync()
        => Task.FromResult<IEnumerable<AttractionRelation>>(_store.Values.ToList());

    public Task<IEnumerable<AttractionRelation>> GetBySourceComponentIdAsync(Guid sourceComponentId)
    {
        var results = _store.Values.Where(r => r.SourceComponentId == sourceComponentId).ToList();
        return Task.FromResult<IEnumerable<AttractionRelation>>(results);
    }

    public Task AddAsync(AttractionRelation relation)
    {
        _store[relation.Id] = relation;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        _store.TryRemove(id, out _);
        return Task.CompletedTask;
    }
}

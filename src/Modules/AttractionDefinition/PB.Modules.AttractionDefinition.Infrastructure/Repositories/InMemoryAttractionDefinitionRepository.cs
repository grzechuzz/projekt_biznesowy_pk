using System.Collections.Concurrent;
using PB.Shared.Domain;
using PB.Modules.AttractionDefinition.Domain.Ports;
using AttractionDefinitionAggregate = PB.Modules.AttractionDefinition.Domain.Aggregates.AttractionDefinition;

namespace PB.Modules.AttractionDefinition.Infrastructure.Repositories;

public class InMemoryAttractionDefinitionRepository : IAttractionDefinitionRepository
{
    private readonly ConcurrentDictionary<Guid, AttractionDefinitionAggregate> _store = new();

    public Task<AttractionDefinitionAggregate?> GetByIdAsync(Guid id)
    {
        _store.TryGetValue(id, out var result);
        return Task.FromResult(result);
    }

    public Task<IEnumerable<AttractionDefinitionAggregate>> GetAllAsync()
        => Task.FromResult<IEnumerable<AttractionDefinitionAggregate>>(_store.Values.ToList());

    public Task<IEnumerable<AttractionDefinitionAggregate>> GetByTagAsync(Tag tag)
    {
        var results = _store.Values.Where(d => d.Tags.Contains(tag)).ToList();
        return Task.FromResult<IEnumerable<AttractionDefinitionAggregate>>(results);
    }

    public Task AddAsync(AttractionDefinitionAggregate definition)
    {
        _store[definition.Id] = definition;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(AttractionDefinitionAggregate definition)
    {
        _store[definition.Id] = definition;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        _store.TryRemove(id, out _);
        return Task.CompletedTask;
    }
}

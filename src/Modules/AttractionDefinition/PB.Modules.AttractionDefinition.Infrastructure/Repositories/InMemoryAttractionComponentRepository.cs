using System.Collections.Concurrent;
using PB.Shared.Domain;
using PB.Modules.AttractionDefinition.Domain.Aggregates;
using PB.Modules.AttractionDefinition.Domain.Ports;
using AttractionDefinitionAggregate = PB.Modules.AttractionDefinition.Domain.Aggregates.AttractionDefinition;

namespace PB.Modules.AttractionDefinition.Infrastructure.Repositories;

public class InMemoryAttractionComponentRepository : IAttractionComponentRepository
{
    private readonly ConcurrentDictionary<Guid, AttractionComponent> _store = new();

    public Task<AttractionComponent?> GetByIdAsync(Guid id)
    {
        _store.TryGetValue(id, out var result);
        return Task.FromResult(result);
    }

    public Task<IEnumerable<AttractionComponent>> GetAllAsync()
        => Task.FromResult<IEnumerable<AttractionComponent>>(_store.Values.ToList());

    public Task<IEnumerable<AttractionDefinitionAggregate>> GetAllDefinitionsAsync()
        => Task.FromResult(_store.Values.OfType<AttractionDefinitionAggregate>());

    public Task<IEnumerable<AttractionDefinitionAggregate>> GetDefinitionsByTagAsync(Tag tag)
        => Task.FromResult(_store.Values.OfType<AttractionDefinitionAggregate>().Where(d => d.Tags.Contains(tag)));

    public Task<IEnumerable<AttractionPackage>> GetAllPackagesAsync()
        => Task.FromResult(_store.Values.OfType<AttractionPackage>());

    public Task AddAsync(AttractionComponent component)
    {
        _store[component.Id] = component;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(AttractionComponent component)
    {
        _store[component.Id] = component;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        _store.TryRemove(id, out _);
        return Task.CompletedTask;
    }
}

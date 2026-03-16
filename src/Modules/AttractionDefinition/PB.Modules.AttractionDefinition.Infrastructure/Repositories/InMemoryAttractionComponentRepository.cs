namespace PB.Modules.AttractionDefinition.Infrastructure.Repositories;

using PB.Modules.AttractionDefinition.Domain.Entities;
using PB.Modules.AttractionDefinition.Domain.Repositories;
using System.Collections.Concurrent;

public class InMemoryAttractionComponentRepository : IAttractionComponentRepository
{
    private readonly ConcurrentDictionary<Guid, AttractionComponent> _store = new();

    public Task<AttractionComponent?> GetByIdAsync(Guid id)
    {
        _store.TryGetValue(id, out var component);
        return Task.FromResult(component);
    }

    public Task<IReadOnlyList<AttractionComponent>> GetAllAsync()
    {
        IReadOnlyList<AttractionComponent> result = _store.Values.ToList();
        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<AttractionComponent>> GetByStatusAsync(AttractionStatus status)
    {
        IReadOnlyList<AttractionComponent> result = _store.Values
            .Where(c => c.Status == status)
            .ToList();
        return Task.FromResult(result);
    }

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
}

using System.Collections.Concurrent;
using PB.Modules.AttractionDefinition.Domain.Aggregates;
using PB.Modules.AttractionDefinition.Domain.Ports;

namespace PB.Modules.AttractionDefinition.Infrastructure.Repositories;

public class InMemoryAttractionPackageRepository : IAttractionPackageRepository
{
    private readonly ConcurrentDictionary<Guid, AttractionPackage> _store = new();

    public Task<AttractionPackage?> GetByIdAsync(Guid id)
    {
        _store.TryGetValue(id, out var result);
        return Task.FromResult(result);
    }

    public Task<IEnumerable<AttractionPackage>> GetAllAsync()
        => Task.FromResult<IEnumerable<AttractionPackage>>(_store.Values.ToList());

    public Task AddAsync(AttractionPackage package)
    {
        _store[package.Id] = package;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(AttractionPackage package)
    {
        _store[package.Id] = package;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        _store.TryRemove(id, out _);
        return Task.CompletedTask;
    }
}

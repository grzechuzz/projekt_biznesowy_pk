namespace PB.Modules.Preference.Infrastructure.Repositories;

using PB.Modules.Preference.Domain.Entities;
using PB.Modules.Preference.Domain.Repositories;
using System.Collections.Concurrent;

public class InMemoryUserPreferenceRepository : IUserPreferenceRepository
{
    private readonly ConcurrentDictionary<Guid, UserPreference> _store = new();

    public Task<UserPreference?> GetByIdAsync(Guid id)
    {
        _store.TryGetValue(id, out var preference);
        return Task.FromResult(preference);
    }

    public Task<IReadOnlyList<UserPreference>> GetAllAsync()
    {
        IReadOnlyList<UserPreference> result = _store.Values.ToList();
        return Task.FromResult(result);
    }

    public Task AddAsync(UserPreference preference)
    {
        _store[preference.Id] = preference;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(UserPreference preference)
    {
        _store[preference.Id] = preference;
        return Task.CompletedTask;
    }
}

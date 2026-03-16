namespace PB.Modules.Preference.Domain.Repositories;

using PB.Modules.Preference.Domain.Entities;

public interface IUserPreferenceRepository
{
    Task<UserPreference?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<UserPreference>> GetAllAsync();
    Task AddAsync(UserPreference preference);
    Task UpdateAsync(UserPreference preference);
}

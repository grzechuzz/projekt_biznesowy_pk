namespace PB.Modules.Preference.Application.Services;

using PB.Modules.Preference.Application.DTOs;

public interface IPreferenceService
{
    Task<UserPreferenceDto> CreateAsync(CreateUserPreferenceDto dto);
    Task<UserPreferenceDto?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<UserPreferenceDto>> GetAllAsync();
    Task<UserPreferenceDto> UpdateAsync(Guid id, UpdateUserPreferenceDto dto);
}

namespace PB.Modules.Preference.Application.Services;

using PB.Modules.Preference.Application.DTOs;
using PB.Modules.Preference.Domain.Entities;
using PB.Modules.Preference.Domain.Repositories;
using PB.Modules.Preference.Domain.ValueObjects;

public class PreferenceService : IPreferenceService
{
    private readonly IUserPreferenceRepository _repository;

    public PreferenceService(IUserPreferenceRepository repository)
    {
        _repository = repository;
    }

    public async Task<UserPreferenceDto> CreateAsync(CreateUserPreferenceDto dto)
    {
        if (!Enum.TryParse<ActivityLevel>(dto.ActivityLevel, true, out var activityLevel))
            throw new InvalidOperationException($"Invalid activity level: {dto.ActivityLevel}. Use: Low, Medium, High.");

        var preference = new UserPreference(
            Guid.NewGuid(),
            dto.UserName,
            new TripDetails(dto.DestinationCity, dto.StartDate, dto.EndDate),
            dto.PreferredCategories,
            new TransportPreference(dto.Walking, dto.Bicycle, dto.PublicTransport, dto.Car),
            activityLevel,
            dto.MaxHoursPerDay);

        await _repository.AddAsync(preference);
        return MapToDto(preference);
    }

    public async Task<UserPreferenceDto?> GetByIdAsync(Guid id)
    {
        var preference = await _repository.GetByIdAsync(id);
        return preference is null ? null : MapToDto(preference);
    }

    public async Task<IReadOnlyList<UserPreferenceDto>> GetAllAsync()
    {
        var preferences = await _repository.GetAllAsync();
        return preferences.Select(MapToDto).ToList();
    }

    public async Task<UserPreferenceDto> UpdateAsync(Guid id, UpdateUserPreferenceDto dto)
    {
        var preference = await _repository.GetByIdAsync(id)
            ?? throw new InvalidOperationException($"Preference {id} not found.");

        if (!Enum.TryParse<ActivityLevel>(dto.ActivityLevel, true, out var activityLevel))
            throw new InvalidOperationException($"Invalid activity level: {dto.ActivityLevel}. Use: Low, Medium, High.");

        preference.Update(
            new TripDetails(dto.DestinationCity, dto.StartDate, dto.EndDate),
            dto.PreferredCategories,
            new TransportPreference(dto.Walking, dto.Bicycle, dto.PublicTransport, dto.Car),
            activityLevel,
            dto.MaxHoursPerDay);

        await _repository.UpdateAsync(preference);
        return MapToDto(preference);
    }

    private static UserPreferenceDto MapToDto(UserPreference p)
    {
        return new UserPreferenceDto(
            p.Id,
            p.UserName,
            p.TripDetails.DestinationCity,
            p.TripDetails.StartDate,
            p.TripDetails.EndDate,
            p.PreferredCategories,
            p.TransportPreference.Walking,
            p.TransportPreference.Bicycle,
            p.TransportPreference.PublicTransport,
            p.TransportPreference.Car,
            p.ActivityLevel.ToString(),
            p.MaxHoursPerDay);
    }
}

namespace PB.Modules.Preference.Application.DTOs;

public record CreateUserPreferenceDto(
    string UserName,
    string DestinationCity,
    DateOnly StartDate,
    DateOnly EndDate,
    List<string> PreferredCategories,
    bool Walking,
    bool Bicycle,
    bool PublicTransport,
    bool Car,
    string ActivityLevel,
    int MaxHoursPerDay);

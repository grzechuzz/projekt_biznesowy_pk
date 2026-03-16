namespace PB.Modules.Preference.Domain.Entities;

using PB.Shared.Domain;
using PB.Modules.Preference.Domain.ValueObjects;

public class UserPreference : AggregateRoot
{
    public string UserName { get; private set; } = string.Empty;
    public TripDetails TripDetails { get; private set; } = null!;
    public List<string> PreferredCategories { get; private set; } = new();
    public TransportPreference TransportPreference { get; private set; } = null!;
    public ActivityLevel ActivityLevel { get; private set; }
    public int MaxHoursPerDay { get; private set; }

    private UserPreference() { }

    public UserPreference(
        Guid id,
        string userName,
        TripDetails tripDetails,
        List<string> preferredCategories,
        TransportPreference transportPreference,
        ActivityLevel activityLevel,
        int maxHoursPerDay) : base(id)
    {
        if (string.IsNullOrWhiteSpace(userName))
            throw new DomainException("User name cannot be empty.");
        if (maxHoursPerDay is < 1 or > 24)
            throw new DomainException("Max hours per day must be between 1 and 24.");

        UserName = userName;
        TripDetails = tripDetails;
        PreferredCategories = preferredCategories;
        TransportPreference = transportPreference;
        ActivityLevel = activityLevel;
        MaxHoursPerDay = maxHoursPerDay;
    }

    public void Update(
        TripDetails tripDetails,
        List<string> preferredCategories,
        TransportPreference transportPreference,
        ActivityLevel activityLevel,
        int maxHoursPerDay)
    {
        if (maxHoursPerDay is < 1 or > 24)
            throw new DomainException("Max hours per day must be between 1 and 24.");

        TripDetails = tripDetails;
        PreferredCategories = preferredCategories;
        TransportPreference = transportPreference;
        ActivityLevel = activityLevel;
        MaxHoursPerDay = maxHoursPerDay;
    }
}

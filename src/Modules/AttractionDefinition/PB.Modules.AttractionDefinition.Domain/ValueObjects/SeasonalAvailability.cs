namespace PB.Modules.AttractionDefinition.Domain.ValueObjects;

using PB.Shared.Domain;

public class SeasonalAvailability : ValueObject
{
    public bool Spring { get; }
    public bool Summer { get; }
    public bool Autumn { get; }
    public bool Winter { get; }

    public static SeasonalAvailability AllYear => new(true, true, true, true);

    public SeasonalAvailability(bool spring, bool summer, bool autumn, bool winter)
    {
        if (!spring && !summer && !autumn && !winter)
            throw new DomainException("Attraction must be available in at least one season.");
        Spring = spring;
        Summer = summer;
        Autumn = autumn;
        Winter = winter;
    }

    public bool IsAvailableIn(Season season) => season switch
    {
        Season.Spring => Spring,
        Season.Summer => Summer,
        Season.Autumn => Autumn,
        Season.Winter => Winter,
        _ => false
    };

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Spring;
        yield return Summer;
        yield return Autumn;
        yield return Winter;
    }
}

public enum Season
{
    Spring,
    Summer,
    Autumn,
    Winter
}

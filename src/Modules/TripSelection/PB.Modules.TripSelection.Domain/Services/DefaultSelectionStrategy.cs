namespace PB.Modules.TripSelection.Domain.Services;

public class DefaultSelectionStrategy : ISelectionStrategy
{
    private const double MustHaveThreshold = 0.7;

    public double CalculateMatchScore(string attractionCategory, bool isEvent, IReadOnlyList<string> preferredCategories)
    {
        double score = 0.0;

        if (preferredCategories.Any(c => c.Equals(attractionCategory, StringComparison.OrdinalIgnoreCase)))
        {
            score += 0.8;
        }

        if (isEvent)
        {
            score += 0.15;
        }

        return Math.Min(score, 1.0);
    }

    public bool IsMustHave(double score) => score >= MustHaveThreshold;
}

namespace PB.Modules.TripSelection.Domain.Services;

public interface ISelectionStrategy
{
    double CalculateMatchScore(string attractionCategory, bool isEvent, IReadOnlyList<string> preferredCategories);
    bool IsMustHave(double score);
}

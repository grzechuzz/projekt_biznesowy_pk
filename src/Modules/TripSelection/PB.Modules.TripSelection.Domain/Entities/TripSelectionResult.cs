namespace PB.Modules.TripSelection.Domain.Entities;

using PB.Shared.Domain;

public class TripSelectionResult : AggregateRoot
{
    public Guid PreferenceId { get; private set; }
    public string DestinationCity { get; private set; } = string.Empty;
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }

    private readonly List<SelectedAttraction> _mustHave = new();
    private readonly List<SelectedAttraction> _optional = new();

    public IReadOnlyList<SelectedAttraction> MustHave => _mustHave.AsReadOnly();
    public IReadOnlyList<SelectedAttraction> Optional => _optional.AsReadOnly();

    public DateTime CreatedAt { get; private set; }

    private TripSelectionResult() { }

    public TripSelectionResult(
        Guid id,
        Guid preferenceId,
        string destinationCity,
        DateOnly startDate,
        DateOnly endDate) : base(id)
    {
        PreferenceId = preferenceId;
        DestinationCity = destinationCity;
        StartDate = startDate;
        EndDate = endDate;
        CreatedAt = DateTime.UtcNow;
    }

    public void AddMustHave(SelectedAttraction attraction)
    {
        _mustHave.Add(attraction);
    }

    public void AddOptional(SelectedAttraction attraction)
    {
        _optional.Add(attraction);
    }
}

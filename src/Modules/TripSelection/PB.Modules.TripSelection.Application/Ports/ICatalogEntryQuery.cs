using PB.Shared.Domain;

namespace PB.Modules.TripSelection.Application.Ports;

public record ConstraintSnapshot(string Type, string Key, decimal? MinValue, decimal? MaxValue, IReadOnlyList<string> AllowedValues);

public record CatalogEntrySnapshot(
    Guid Id,
    string Name,
    string Description,
    Guid AttractionComponentId,
    IReadOnlySet<Tag> Tags,
    string City,
    bool IsEvent,
    string Status,
    IReadOnlyList<ConstraintSnapshot> Constraints);

public interface ICatalogEntryQuery
{
    Task<CatalogEntrySnapshot?> GetByIdAsync(Guid id);
    Task<IEnumerable<CatalogEntrySnapshot>> GetByAttractionComponentIdAsync(Guid attractionComponentId);
}

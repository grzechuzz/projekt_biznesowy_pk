using PB.Shared.Domain;

namespace PB.Modules.TripSelection.Application.Ports;

public record CatalogEntrySnapshot(
    Guid Id,
    string Name,
    string Description,
    Guid AttractionDefinitionId,
    Guid? VariantId,
    IReadOnlySet<Tag> Tags,
    string City,
    bool IsEvent,
    string Status);

public interface ICatalogEntryQuery
{
    Task<CatalogEntrySnapshot?> GetByIdAsync(Guid id);
}

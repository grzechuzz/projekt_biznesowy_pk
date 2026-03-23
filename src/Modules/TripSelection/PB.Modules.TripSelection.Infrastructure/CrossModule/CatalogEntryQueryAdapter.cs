using PB.Shared.Domain;
using PB.Modules.TripSelection.Application.Ports;
using PB.Modules.Catalog.Application.Services;

namespace PB.Modules.TripSelection.Infrastructure.CrossModule;

public class CatalogEntryQueryAdapter : ICatalogEntryQuery
{
    private readonly ICatalogService _catalogService;

    public CatalogEntryQueryAdapter(ICatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    public async Task<CatalogEntrySnapshot?> GetByIdAsync(Guid id)
    {
        var dto = await _catalogService.GetByIdAsync(id);
        if (dto == null) return null;

        var tags = new HashSet<Tag>(dto.Tags.Select(t => new Tag(t.Name, t.Group)));

        return new CatalogEntrySnapshot(
            dto.Id,
            dto.Name,
            dto.Description,
            dto.AttractionDefinitionId,
            dto.VariantId,
            tags,
            dto.Location.City,
            dto.IsEvent,
            dto.Status);
    }
}

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
        return dto == null ? null : MapToSnapshot(dto);
    }

    public async Task<IEnumerable<CatalogEntrySnapshot>> GetByAttractionComponentIdAsync(Guid attractionComponentId)
    {
        var dtos = await _catalogService.GetByAttractionComponentIdAsync(attractionComponentId);
        return dtos.Select(MapToSnapshot);
    }

    private static CatalogEntrySnapshot MapToSnapshot(Catalog.Application.DTOs.CatalogEntryDto dto)
    {
        var tags = new HashSet<Tag>(dto.Tags.Select(t => new Tag(t.Name, t.Group)));
        var constraints = dto.Constraints
            .Select(c => new ConstraintSnapshot(c.Type, c.Key, c.MinValue, c.MaxValue,
                (IReadOnlyList<string>)(c.AllowedValues ?? new List<string>())))
            .ToList();

        return new CatalogEntrySnapshot(
            dto.Id, dto.Name, dto.Description,
            dto.AttractionComponentId,
            tags, dto.Location.City, dto.IsEvent, dto.Status, constraints);
    }
}

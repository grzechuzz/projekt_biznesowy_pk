using PB.Modules.Catalog.Application.DTOs;

namespace PB.Modules.Catalog.Application.Services;

public interface ICatalogService
{
    Task<CatalogEntryDto> CreateAsync(CreateCatalogEntryDto dto);
    Task<CatalogEntryDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<CatalogEntryDto>> GetByAttractionComponentIdAsync(Guid attractionComponentId);
    Task<IEnumerable<CatalogEntryDto>> SearchAsync(string? city, DateOnly? from, DateOnly? to, IEnumerable<string>? tags, string? status);
    Task<CatalogEntryDto> UpdateAsync(Guid id, UpdateCatalogEntryDto dto);
    Task DeleteAsync(Guid id);
    Task<CatalogEntryDto> AddPricingPeriodAsync(Guid id, AddPricingPeriodDto dto);
    Task<CatalogEntryDto> RemovePricingPeriodAsync(Guid id, int index);
    Task<CatalogEntryDto> CancelAsync(Guid id);
    Task<CatalogEntryDto> MarkAsSoldOutAsync(Guid id);
    Task<CatalogEntryDto> MarkAsAvailableAsync(Guid id);
    Task<CatalogEntryDto> AddTagAsync(Guid id, TagDto tag);
    Task<CatalogEntryDto> RemoveTagAsync(Guid id, TagDto tag);
}

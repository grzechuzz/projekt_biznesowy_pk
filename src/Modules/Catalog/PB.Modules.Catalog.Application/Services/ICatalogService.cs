namespace PB.Modules.Catalog.Application.Services;

using PB.Modules.Catalog.Application.DTOs;

public interface ICatalogService
{
    Task<CatalogEntryDto> CreateAsync(CreateCatalogEntryDto dto);
    Task<CatalogEntryDto?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<CatalogEntryDto>> SearchAsync(string? city, DateOnly? from, DateOnly? to, string? category);
    Task DeleteAsync(Guid id);
}

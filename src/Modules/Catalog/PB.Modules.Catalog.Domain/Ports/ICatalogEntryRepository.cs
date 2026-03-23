using PB.Modules.Catalog.Domain.Aggregates;
using PB.Modules.Catalog.Domain.Enums;

namespace PB.Modules.Catalog.Domain.Ports;

public interface ICatalogEntryRepository
{
    Task<CatalogEntry?> GetByIdAsync(Guid id);
    Task<IEnumerable<CatalogEntry>> GetAllAsync();
    Task<IEnumerable<CatalogEntry>> SearchAsync(string? city, DateOnly? from, DateOnly? to, IEnumerable<string>? tags, CatalogEntryStatus? status);
    Task<IEnumerable<CatalogEntry>> GetByAttractionDefinitionIdAsync(Guid attractionDefinitionId);
    Task AddAsync(CatalogEntry entry);
    Task UpdateAsync(CatalogEntry entry);
    Task DeleteAsync(Guid id);
}

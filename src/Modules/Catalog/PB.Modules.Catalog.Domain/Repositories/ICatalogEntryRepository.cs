namespace PB.Modules.Catalog.Domain.Repositories;

using PB.Modules.Catalog.Domain.Entities;

public interface ICatalogEntryRepository
{
    Task<CatalogEntry?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<CatalogEntry>> GetAllAsync();
    Task<IReadOnlyList<CatalogEntry>> SearchAsync(string? city, DateOnly? from, DateOnly? to, string? category);
    Task AddAsync(CatalogEntry entry);
    Task DeleteAsync(Guid id);
}

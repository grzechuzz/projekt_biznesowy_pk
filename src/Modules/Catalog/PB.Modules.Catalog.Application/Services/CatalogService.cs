namespace PB.Modules.Catalog.Application.Services;

using PB.Modules.Catalog.Application.DTOs;
using PB.Modules.Catalog.Domain.Entities;
using PB.Modules.Catalog.Domain.Repositories;
using PB.Modules.Catalog.Domain.ValueObjects;

public class CatalogService : ICatalogService
{
    private readonly ICatalogEntryRepository _repository;

    public CatalogService(ICatalogEntryRepository repository)
    {
        _repository = repository;
    }

    public async Task<CatalogEntryDto> CreateAsync(CreateCatalogEntryDto dto)
    {
        var entry = new CatalogEntry(
            Guid.NewGuid(),
            dto.AttractionDefinitionId,
            dto.Name,
            dto.Description,
            dto.Category,
            new CatalogLocation(dto.City, dto.Address),
            new DateRange(dto.ValidFrom, dto.ValidTo),
            dto.OpeningTime.HasValue && dto.ClosingTime.HasValue
                ? new CatalogOpeningHours(dto.OpeningTime.Value, dto.ClosingTime.Value)
                : null,
            dto.IsEvent);

        await _repository.AddAsync(entry);
        return MapToDto(entry);
    }

    public async Task<CatalogEntryDto?> GetByIdAsync(Guid id)
    {
        var entry = await _repository.GetByIdAsync(id);
        return entry is null ? null : MapToDto(entry);
    }

    public async Task<IReadOnlyList<CatalogEntryDto>> SearchAsync(string? city, DateOnly? from, DateOnly? to, string? category)
    {
        var entries = await _repository.SearchAsync(city, from, to, category);
        return entries.Select(MapToDto).ToList();
    }

    public async Task DeleteAsync(Guid id)
    {
        await _repository.DeleteAsync(id);
    }

    private static CatalogEntryDto MapToDto(CatalogEntry entry)
    {
        return new CatalogEntryDto(
            entry.Id,
            entry.AttractionDefinitionId,
            entry.Name,
            entry.Description,
            entry.Category,
            entry.Location.City,
            entry.Location.Address,
            entry.ValidPeriod.From,
            entry.ValidPeriod.To,
            entry.OpeningHours?.Open,
            entry.OpeningHours?.Close,
            entry.IsEvent);
    }
}

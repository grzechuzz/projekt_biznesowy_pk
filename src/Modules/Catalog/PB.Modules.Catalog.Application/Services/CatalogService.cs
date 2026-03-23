using PB.Shared.Domain;
using PB.Modules.Catalog.Application.DTOs;
using PB.Modules.Catalog.Domain.Aggregates;
using PB.Modules.Catalog.Domain.Enums;
using PB.Modules.Catalog.Domain.Ports;
using PB.Modules.Catalog.Domain.ValueObjects;

namespace PB.Modules.Catalog.Application.Services;

public class CatalogService : ICatalogService
{
    private readonly ICatalogEntryRepository _repository;

    public CatalogService(ICatalogEntryRepository repository)
    {
        _repository = repository;
    }

    public async Task<CatalogEntryDto> CreateAsync(CreateCatalogEntryDto dto)
    {
        var location = new CatalogLocation(dto.Location.City, dto.Location.Address);
        var dateRange = new DateRange(dto.DateRange.From, dto.DateRange.To);
        var tags = dto.Tags?.Select(t => new Tag(t.Name, t.Group));

        var entry = new CatalogEntry(
            dto.AttractionDefinitionId, dto.VariantId,
            dto.Name, dto.Description,
            location, dateRange, dto.IsEvent, tags);

        if (dto.OpeningHours != null)
            entry.SetOpeningHours(new CatalogOpeningHours(dto.OpeningHours.Open, dto.OpeningHours.Close));

        await _repository.AddAsync(entry);
        return MapToDto(entry);
    }

    public async Task<CatalogEntryDto?> GetByIdAsync(Guid id)
    {
        var entry = await _repository.GetByIdAsync(id);
        return entry == null ? null : MapToDto(entry);
    }

    public async Task<IEnumerable<CatalogEntryDto>> SearchAsync(string? city, DateOnly? from, DateOnly? to, IEnumerable<string>? tags, string? status)
    {
        CatalogEntryStatus? statusEnum = null;
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<CatalogEntryStatus>(status, true, out var parsed))
            statusEnum = parsed;

        var results = await _repository.SearchAsync(city, from, to, tags, statusEnum);
        return results.Select(MapToDto);
    }

    public async Task<CatalogEntryDto> UpdateAsync(Guid id, UpdateCatalogEntryDto dto)
    {
        var entry = await _repository.GetByIdAsync(id)
            ?? throw new DomainException($"CatalogEntry {id} not found");

        var location = new CatalogLocation(dto.Location.City, dto.Location.Address);
        var dateRange = new DateRange(dto.DateRange.From, dto.DateRange.To);
        entry.Update(dto.Name, dto.Description, location, dateRange, dto.IsEvent);

        if (dto.OpeningHours != null)
            entry.SetOpeningHours(new CatalogOpeningHours(dto.OpeningHours.Open, dto.OpeningHours.Close));
        else
            entry.SetOpeningHours(null);

        await _repository.UpdateAsync(entry);
        return MapToDto(entry);
    }

    public async Task DeleteAsync(Guid id)
    {
        var entry = await _repository.GetByIdAsync(id)
            ?? throw new DomainException($"CatalogEntry {id} not found");
        await _repository.DeleteAsync(id);
    }

    public async Task<CatalogEntryDto> AddPricingPeriodAsync(Guid id, AddPricingPeriodDto dto)
    {
        var entry = await _repository.GetByIdAsync(id)
            ?? throw new DomainException($"CatalogEntry {id} not found");

        var dateRange = new DateRange(dto.From, dto.To);
        var price = new Money(dto.Price.Amount, dto.Price.Currency);
        var discounts = dto.Discounts?.Select(d => new Discount(
            d.Description, d.PercentOff,
            d.AmountOff != null ? new Money(d.AmountOff.Amount, d.AmountOff.Currency) : null,
            d.Condition)).ToList();

        var period = new PricingPeriod(dateRange, price, discounts);
        entry.AddPricingPeriod(period);
        await _repository.UpdateAsync(entry);
        return MapToDto(entry);
    }

    public async Task<CatalogEntryDto> RemovePricingPeriodAsync(Guid id, int index)
    {
        var entry = await _repository.GetByIdAsync(id)
            ?? throw new DomainException($"CatalogEntry {id} not found");
        entry.RemovePricingPeriodAt(index);
        await _repository.UpdateAsync(entry);
        return MapToDto(entry);
    }

    public async Task<CatalogEntryDto> CancelAsync(Guid id)
    {
        var entry = await _repository.GetByIdAsync(id)
            ?? throw new DomainException($"CatalogEntry {id} not found");
        entry.Cancel();
        await _repository.UpdateAsync(entry);
        return MapToDto(entry);
    }

    public async Task<CatalogEntryDto> MarkAsSoldOutAsync(Guid id)
    {
        var entry = await _repository.GetByIdAsync(id)
            ?? throw new DomainException($"CatalogEntry {id} not found");
        entry.MarkAsSoldOut();
        await _repository.UpdateAsync(entry);
        return MapToDto(entry);
    }

    public async Task<CatalogEntryDto> MarkAsAvailableAsync(Guid id)
    {
        var entry = await _repository.GetByIdAsync(id)
            ?? throw new DomainException($"CatalogEntry {id} not found");
        entry.MarkAsAvailable();
        await _repository.UpdateAsync(entry);
        return MapToDto(entry);
    }

    public async Task<CatalogEntryDto> AddTagAsync(Guid id, TagDto tag)
    {
        var entry = await _repository.GetByIdAsync(id)
            ?? throw new DomainException($"CatalogEntry {id} not found");
        entry.AddTag(new Tag(tag.Name, tag.Group));
        await _repository.UpdateAsync(entry);
        return MapToDto(entry);
    }

    public async Task<CatalogEntryDto> RemoveTagAsync(Guid id, TagDto tag)
    {
        var entry = await _repository.GetByIdAsync(id)
            ?? throw new DomainException($"CatalogEntry {id} not found");
        entry.RemoveTag(new Tag(tag.Name, tag.Group));
        await _repository.UpdateAsync(entry);
        return MapToDto(entry);
    }

    // --- Mappers ---

    private static CatalogEntryDto MapToDto(CatalogEntry e)
    {
        return new CatalogEntryDto(
            e.Id,
            e.AttractionDefinitionId,
            e.VariantId,
            e.Name,
            e.Description,
            e.Tags.Select(t => new TagDto(t.Name, t.Group)).ToList(),
            new CatalogLocationDto(e.Location.City, e.Location.Address),
            new DateRangeDto(e.DateRange.From, e.DateRange.To),
            e.OpeningHours != null ? new CatalogOpeningHoursDto(e.OpeningHours.Open, e.OpeningHours.Close) : null,
            e.IsEvent,
            e.Status.ToString(),
            e.PricingPeriods.Select(p => new PricingPeriodDto(
                p.DateRange.From,
                p.DateRange.To,
                new MoneyDto(p.Price.Amount, p.Price.Currency),
                p.Discounts.Select(d => new DiscountDto(
                    d.Description, d.PercentOff,
                    d.AmountOff != null ? new MoneyDto(d.AmountOff.Amount, d.AmountOff.Currency) : null,
                    d.Condition)).ToList()
            )).ToList());
    }
}

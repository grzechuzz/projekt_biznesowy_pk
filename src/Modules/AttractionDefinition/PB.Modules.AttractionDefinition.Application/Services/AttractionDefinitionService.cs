using PB.Shared.Domain;
using PB.Modules.AttractionDefinition.Application.DTOs;
using PB.Modules.AttractionDefinition.Domain.Ports;
using PB.Modules.AttractionDefinition.Domain.ValueObjects;
using PB.Modules.AttractionDefinition.Domain.Enums;
using AttractionDefinitionAggregate = PB.Modules.AttractionDefinition.Domain.Aggregates.AttractionDefinition;
using AttractionVariantEntity = PB.Modules.AttractionDefinition.Domain.Entities.AttractionVariant;

namespace PB.Modules.AttractionDefinition.Application.Services;

public class AttractionDefinitionService : IAttractionDefinitionService
{
    private readonly IAttractionComponentRepository _repository;

    public AttractionDefinitionService(IAttractionComponentRepository repository)
    {
        _repository = repository;
    }

    public async Task<AttractionDefinitionDto> CreateAsync(CreateAttractionDefinitionDto dto)
    {
        var definition = new AttractionDefinitionAggregate(dto.Name, dto.Description);

        if (dto.Tags != null)
            foreach (var t in dto.Tags)
                definition.AddTag(MapTag(t));

        if (dto.Location != null)
            definition.SetLocation(MapLocation(dto.Location));

        if (dto.OpeningHours != null)
            definition.SetOpeningHours(MapOpeningHours(dto.OpeningHours));

        if (dto.SeasonalAvailability != null)
            definition.SetSeasonalAvailability(MapSeasonalAvailability(dto.SeasonalAvailability));

        await _repository.AddAsync(definition);
        return MapToDto(definition);
    }

    public async Task<AttractionDefinitionDto?> GetByIdAsync(Guid id)
    {
        var component = await _repository.GetByIdAsync(id);
        return component is AttractionDefinitionAggregate definition ? MapToDto(definition) : null;
    }

    public async Task<IEnumerable<AttractionDefinitionDto>> GetAllAsync(string? tagFilter = null, string? cityFilter = null, bool? isComplete = null)
    {
        var all = await _repository.GetAllDefinitionsAsync();
        var filtered = all.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(tagFilter))
            filtered = filtered.Where(d => d.Tags.Any(t => t.Name.Contains(tagFilter.ToLower())));

        if (!string.IsNullOrWhiteSpace(cityFilter))
            filtered = filtered.Where(d => d.Location != null &&
                d.Location.City.Contains(cityFilter, StringComparison.OrdinalIgnoreCase));

        if (isComplete.HasValue)
            filtered = filtered.Where(d => d.IsComplete == isComplete.Value);

        return filtered.Select(MapToDto);
    }

    public async Task<AttractionDefinitionDto> UpdateAsync(Guid id, UpdateAttractionDefinitionDto dto)
    {
        var definition = (await _repository.GetByIdAsync(id) as AttractionDefinitionAggregate)
            ?? throw new DomainException($"AttractionDefinition {id} not found");
        definition.Update(dto.Name, dto.Description);
        await _repository.UpdateAsync(definition);
        return MapToDto(definition);
    }

    public async Task DeleteAsync(Guid id)
    {
        var definition = (await _repository.GetByIdAsync(id) as AttractionDefinitionAggregate)
            ?? throw new DomainException($"AttractionDefinition {id} not found");
        await _repository.DeleteAsync(id);
    }

    public async Task<AttractionDefinitionDto> AddVariantAsync(Guid id, AddVariantDto dto)
    {
        var definition = (await _repository.GetByIdAsync(id) as AttractionDefinitionAggregate)
            ?? throw new DomainException($"AttractionDefinition {id} not found");

        var variant = definition.AddVariant(dto.Name, dto.Description, dto.DurationMinutes);

        if (dto.AdditionalTags != null)
            foreach (var t in dto.AdditionalTags)
                variant.AddTag(MapTag(t));

        if (dto.Constraints != null)
            foreach (var c in dto.Constraints)
                variant.AddConstraint(MapConstraint(c));

        await _repository.UpdateAsync(definition);
        return MapToDto(definition);
    }

    public async Task<AttractionDefinitionDto> UpdateVariantAsync(Guid id, Guid variantId, UpdateVariantDto dto)
    {
        var definition = (await _repository.GetByIdAsync(id) as AttractionDefinitionAggregate)
            ?? throw new DomainException($"AttractionDefinition {id} not found");
        var variant = definition.GetVariant(variantId);
        variant.Update(dto.Name, dto.Description, dto.DurationMinutes);
        await _repository.UpdateAsync(definition);
        return MapToDto(definition);
    }

    public async Task<AttractionDefinitionDto> RemoveVariantAsync(Guid id, Guid variantId)
    {
        var definition = (await _repository.GetByIdAsync(id) as AttractionDefinitionAggregate)
            ?? throw new DomainException($"AttractionDefinition {id} not found");
        definition.RemoveVariant(variantId);
        await _repository.UpdateAsync(definition);
        return MapToDto(definition);
    }

    public async Task<AttractionDefinitionDto> AddTagAsync(Guid id, TagDto tag)
    {
        var definition = (await _repository.GetByIdAsync(id) as AttractionDefinitionAggregate)
            ?? throw new DomainException($"AttractionDefinition {id} not found");
        definition.AddTag(MapTag(tag));
        await _repository.UpdateAsync(definition);
        return MapToDto(definition);
    }

    public async Task<AttractionDefinitionDto> RemoveTagAsync(Guid id, TagDto tag)
    {
        var definition = (await _repository.GetByIdAsync(id) as AttractionDefinitionAggregate)
            ?? throw new DomainException($"AttractionDefinition {id} not found");
        definition.RemoveTag(MapTag(tag));
        await _repository.UpdateAsync(definition);
        return MapToDto(definition);
    }

    // --- Mappers ---

    private static Tag MapTag(TagDto dto) => new Tag(dto.Name, dto.Group);
    private static TagDto MapTagDto(Tag tag) => new TagDto(tag.Name, tag.Group);

    private static Location MapLocation(LocationDto dto) =>
        new Location(dto.City, dto.Address, dto.Latitude, dto.Longitude);

    private static LocationDto MapLocationDto(Location loc) =>
        new LocationDto(loc.City, loc.Address, loc.Latitude, loc.Longitude);

    private static OpeningHours MapOpeningHours(OpeningHoursDto dto) =>
        new OpeningHours(dto.Open, dto.Close);

    private static OpeningHoursDto MapOpeningHoursDto(OpeningHours oh) =>
        new OpeningHoursDto(oh.Open, oh.Close);

    private static SeasonalAvailability MapSeasonalAvailability(SeasonalAvailabilityDto dto) =>
        new SeasonalAvailability(dto.Spring, dto.Summer, dto.Autumn, dto.Winter);

    private static SeasonalAvailabilityDto MapSeasonalAvailabilityDto(SeasonalAvailability sa) =>
        new SeasonalAvailabilityDto(sa.Spring, sa.Summer, sa.Autumn, sa.Winter);

    private static Constraint MapConstraint(ConstraintDto dto)
    {
        return dto.Type.ToLower() switch
        {
            "range" => Constraint.Range(dto.Key, dto.MinValue!.Value, dto.MaxValue!.Value),
            "min" => Constraint.Min(dto.Key, dto.MinValue!.Value),
            "max" => Constraint.Max(dto.Key, dto.MaxValue!.Value),
            "oneof" => Constraint.OneOf(dto.Key, dto.AllowedValues ?? new List<string>()),
            "requireddaysahead" => Constraint.RequiredDaysAhead((int)dto.MinValue!.Value),
            _ => throw new DomainException($"Unknown constraint type: {dto.Type}")
        };
    }

    private static ConstraintDto MapConstraintDto(Constraint c) =>
        new ConstraintDto(c.Type.ToString(), c.Key, c.MinValue, c.MaxValue, c.AllowedValues.ToList());

    private static AttractionVariantDto MapVariantDto(AttractionVariantEntity v) =>
        new AttractionVariantDto(
            v.Id,
            v.Name,
            v.Description,
            v.AdditionalTags.Select(MapTagDto).ToList(),
            v.Constraints.Select(MapConstraintDto).ToList(),
            v.DurationMinutes);

    private static AttractionDefinitionDto MapToDto(AttractionDefinitionAggregate d) =>
        new AttractionDefinitionDto(
            d.Id,
            d.Name,
            d.Description,
            d.Tags.Select(MapTagDto).ToList(),
            d.Location != null ? MapLocationDto(d.Location) : null,
            d.OpeningHours != null ? MapOpeningHoursDto(d.OpeningHours) : null,
            d.SeasonalAvailability != null ? MapSeasonalAvailabilityDto(d.SeasonalAvailability) : null,
            d.Variants.Select(MapVariantDto).ToList(),
            d.IsComplete);
}

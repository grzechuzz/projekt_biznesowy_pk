using PB.Modules.AttractionDefinition.Application.DTOs;
using PB.Modules.AttractionDefinition.Domain.Aggregates;
using PB.Modules.AttractionDefinition.Domain.Enums;
using PB.Modules.AttractionDefinition.Domain.Ports;
using PB.Modules.AttractionDefinition.Domain.ValueObjects;
using PB.Shared.Domain;
using AttractionDefinitionAggregate = PB.Modules.AttractionDefinition.Domain.Aggregates.AttractionDefinition;
using AttractionPackageAggregate = PB.Modules.AttractionDefinition.Domain.Aggregates.AttractionPackage;

namespace PB.Modules.AttractionDefinition.Application.Services;

public class AttractionComponentService : IAttractionComponentService
{
    private readonly IAttractionComponentRepository _repository;

    public AttractionComponentService(IAttractionComponentRepository repository)
    {
        _repository = repository;
    }

    public async Task<AttractionComponentDto> CreateAsync(CreateAttractionComponentDto dto)
    {
        AttractionComponent component = dto.Type.ToLowerInvariant() switch
        {
            "attraction" => CreateAttraction(dto),
            "package" => CreatePackage(dto),
            _ => throw new DomainException($"Unknown component type: {dto.Type}")
        };

        await _repository.AddAsync(component);
        return MapToDto(component);
    }

    public async Task<AttractionComponentDto?> GetByIdAsync(Guid id)
    {
        var component = await _repository.GetByIdAsync(id);
        return component == null ? null : MapToDto(component);
    }

    public async Task<IEnumerable<AttractionComponentDto>> GetAllAsync(string? type = null, string? tagFilter = null, string? cityFilter = null, bool? isComplete = null)
    {
        var components = await _repository.GetAllAsync();
        var filtered = components.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(type))
        {
            filtered = type.ToLowerInvariant() switch
            {
                "attraction" => filtered.OfType<AttractionDefinitionAggregate>(),
                "package" => filtered.OfType<AttractionPackageAggregate>(),
                _ => throw new DomainException($"Unknown component type: {type}")
            };
        }

        if (!string.IsNullOrWhiteSpace(tagFilter))
            filtered = filtered.Where(c => c.Tags.Any(t => t.Name.Contains(tagFilter, StringComparison.OrdinalIgnoreCase)));

        if (!string.IsNullOrWhiteSpace(cityFilter))
            filtered = filtered.Where(c =>
                c is AttractionDefinitionAggregate attraction &&
                attraction.Location != null &&
                attraction.Location.City.Contains(cityFilter, StringComparison.OrdinalIgnoreCase));

        if (isComplete.HasValue)
        {
            filtered = filtered.Where(c =>
                c is AttractionDefinitionAggregate attraction &&
                attraction.IsComplete == isComplete.Value);
        }

        return filtered.Select(MapToDto);
    }

    public async Task<AttractionComponentDto> UpdateAsync(Guid id, UpdateAttractionComponentDto dto)
    {
        var component = await _repository.GetByIdAsync(id)
            ?? throw new DomainException($"AttractionComponent {id} not found");

        switch (component)
        {
            case AttractionDefinitionAggregate attraction:
                attraction.Update(dto.Name, dto.Description);
                if (dto.Location != null)
                    attraction.SetLocation(MapLocation(dto.Location));
                attraction.SetOpeningHours(dto.OpeningHours != null ? MapOpeningHours(dto.OpeningHours) : null);
                if (dto.Tags != null)
                    SyncTags(attraction, dto.Tags);
                await _repository.UpdateAsync(attraction);
                return MapToDto(attraction);

            case AttractionPackageAggregate package:
                var selectionRule = dto.SelectionRule != null
                    ? MapSelectionRule(dto.SelectionRule)
                    : package.SelectionRule;
                package.Update(dto.Name, dto.Description, selectionRule);
                if (dto.Tags != null)
                    SyncTags(package, dto.Tags);
                if (dto.ComponentIds != null)
                    SyncPackageComponents(package, dto.ComponentIds);
                await _repository.UpdateAsync(package);
                return MapToDto(package);

            default:
                throw new DomainException($"Unsupported component type: {component.GetType().Name}");
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        var component = await _repository.GetByIdAsync(id)
            ?? throw new DomainException($"AttractionComponent {id} not found");
        await _repository.DeleteAsync(component.Id);
    }

    public async Task<AttractionComponentDto> AddTagAsync(Guid id, TagDto tag)
    {
        var component = await _repository.GetByIdAsync(id)
            ?? throw new DomainException($"AttractionComponent {id} not found");
        component.AddTag(MapTag(tag));
        await _repository.UpdateAsync(component);
        return MapToDto(component);
    }

    public async Task<AttractionComponentDto> RemoveTagAsync(Guid id, TagDto tag)
    {
        var component = await _repository.GetByIdAsync(id)
            ?? throw new DomainException($"AttractionComponent {id} not found");
        component.RemoveTag(MapTag(tag));
        await _repository.UpdateAsync(component);
        return MapToDto(component);
    }

    public async Task<AttractionComponentDto> AddComponentAsync(Guid id, Guid componentId)
    {
        var package = await GetPackageAsync(id);
        package.AddComponent(componentId);
        await _repository.UpdateAsync(package);
        return MapToDto(package);
    }

    public async Task<AttractionComponentDto> RemoveComponentAsync(Guid id, Guid componentId)
    {
        var package = await GetPackageAsync(id);
        package.RemoveComponent(componentId);
        await _repository.UpdateAsync(package);
        return MapToDto(package);
    }

    private static AttractionDefinitionAggregate CreateAttraction(CreateAttractionComponentDto dto)
    {
        var attraction = new AttractionDefinitionAggregate(dto.Name, dto.Description);
        if (dto.Tags != null)
            foreach (var tag in dto.Tags)
                attraction.AddTag(MapTag(tag));
        if (dto.Location != null)
            attraction.SetLocation(MapLocation(dto.Location));
        if (dto.OpeningHours != null)
            attraction.SetOpeningHours(MapOpeningHours(dto.OpeningHours));
        return attraction;
    }

    private static AttractionPackageAggregate CreatePackage(CreateAttractionComponentDto dto)
    {
        if (dto.SelectionRule == null)
            throw new DomainException("Package requires SelectionRule");

        var package = new AttractionPackageAggregate(dto.Name, dto.Description, MapSelectionRule(dto.SelectionRule));
        if (dto.Tags != null)
            foreach (var tag in dto.Tags)
                package.AddTag(MapTag(tag));
        if (dto.ComponentIds != null)
            foreach (var componentId in dto.ComponentIds)
                package.AddComponent(componentId);
        return package;
    }

    private async Task<AttractionPackageAggregate> GetPackageAsync(Guid id)
    {
        var component = await _repository.GetByIdAsync(id)
            ?? throw new DomainException($"AttractionComponent {id} not found");
        return component as AttractionPackageAggregate
            ?? throw new DomainException($"AttractionComponent {id} is not a package");
    }

    private static void SyncTags(AttractionComponent component, IEnumerable<TagDto> tags)
    {
        var desired = tags.Select(MapTag).ToHashSet();
        foreach (var existing in component.Tags.Except(desired).ToList())
            component.RemoveTag(existing);
        foreach (var tag in desired.Except(component.Tags).ToList())
            component.AddTag(tag);
    }

    private static void SyncPackageComponents(AttractionPackageAggregate package, IEnumerable<Guid> componentIds)
    {
        var desired = componentIds.ToHashSet();
        foreach (var existing in package.ComponentIds.Where(id => !desired.Contains(id)).ToList())
            package.RemoveComponent(existing);
        foreach (var componentId in desired.Where(id => !package.ComponentIds.Contains(id)))
            package.AddComponent(componentId);
    }

    private static Tag MapTag(TagDto dto) => new(dto.Name, dto.Group);

    private static Location MapLocation(LocationDto dto) =>
        new(dto.City, dto.Address, dto.Latitude, dto.Longitude);

    private static OpeningHours MapOpeningHours(OpeningHoursDto dto) =>
        new(dto.Open, dto.Close);

    private static SelectionRule MapSelectionRule(SelectionRuleDto dto) =>
        dto.Type.ToLowerInvariant() switch
        {
            "all" => SelectionRule.All(),
            "pickn" => SelectionRule.PickN(dto.Count ?? throw new DomainException("PickN requires Count")),
            _ => throw new DomainException($"Unknown selection rule type: {dto.Type}")
        };

    private static SelectionRuleDto MapSelectionRuleDto(SelectionRule rule) =>
        new(rule.Type.ToString(), rule.Count);

    private static TagDto MapTagDto(Tag tag) => new(tag.Name, tag.Group);

    private static LocationDto MapLocationDto(Location location) =>
        new(location.City, location.Address, location.Latitude, location.Longitude);

    private static OpeningHoursDto MapOpeningHoursDto(OpeningHours openingHours) =>
        new(openingHours.Open, openingHours.Close);

    private static AttractionComponentDto MapToDto(AttractionComponent component) =>
        component switch
        {
            AttractionDefinitionAggregate attraction => new AttractionComponentDto(
                attraction.Id,
                "attraction",
                attraction.Name,
                attraction.Description,
                attraction.Tags.Select(MapTagDto).ToList(),
                attraction.Location != null ? MapLocationDto(attraction.Location) : null,
                attraction.OpeningHours != null ? MapOpeningHoursDto(attraction.OpeningHours) : null,
                null,
                null,
                attraction.IsComplete),
            AttractionPackageAggregate package => new AttractionComponentDto(
                package.Id,
                "package",
                package.Name,
                package.Description,
                package.Tags.Select(MapTagDto).ToList(),
                null,
                null,
                MapSelectionRuleDto(package.SelectionRule),
                package.ComponentIds.ToList(),
                null),
            _ => throw new DomainException($"Unsupported component type: {component.GetType().Name}")
        };
}

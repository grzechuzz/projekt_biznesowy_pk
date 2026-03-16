namespace PB.Modules.AttractionDefinition.Application.Services;

using PB.Modules.AttractionDefinition.Application.DTOs;
using PB.Modules.AttractionDefinition.Domain.Entities;
using PB.Modules.AttractionDefinition.Domain.Repositories;
using PB.Modules.AttractionDefinition.Domain.ValueObjects;

public class AttractionDefinitionService : IAttractionDefinitionService
{
    private readonly IAttractionComponentRepository _repository;

    public AttractionDefinitionService(IAttractionComponentRepository repository)
    {
        _repository = repository;
    }

    public async Task<AttractionComponentDto> CreateSingleAsync(CreateSingleAttractionDto dto)
    {
        var attraction = new SingleAttraction(
            Guid.NewGuid(),
            dto.Name,
            dto.Description,
            new Category(dto.Category),
            new Location(dto.City, dto.Address, dto.Latitude, dto.Longitude),
            dto.OpeningTime.HasValue && dto.ClosingTime.HasValue
                ? new OpeningHours(dto.OpeningTime.Value, dto.ClosingTime.Value)
                : null,
            new SeasonalAvailability(dto.AvailableSpring, dto.AvailableSummer, dto.AvailableAutumn, dto.AvailableWinter));

        await _repository.AddAsync(attraction);
        return MapToDto(attraction);
    }

    public async Task<AttractionComponentDto> CreateGroupAsync(CreateAttractionGroupDto dto)
    {
        var group = new AttractionGroup(Guid.NewGuid(), dto.Name, dto.Description);

        foreach (var childId in dto.ChildIds)
        {
            var child = await _repository.GetByIdAsync(childId)
                ?? throw new InvalidOperationException($"Attraction component {childId} not found.");
            group.AddChild(child);
        }

        await _repository.AddAsync(group);
        return MapToDto(group);
    }

    public async Task<AttractionComponentDto?> GetByIdAsync(Guid id)
    {
        var component = await _repository.GetByIdAsync(id);
        return component is null ? null : MapToDto(component);
    }

    public async Task<IReadOnlyList<AttractionComponentDto>> GetAllAsync(string? statusFilter = null)
    {
        IReadOnlyList<AttractionComponent> components;

        if (statusFilter is not null && Enum.TryParse<AttractionStatus>(statusFilter, true, out var status))
        {
            components = await _repository.GetByStatusAsync(status);
        }
        else
        {
            components = await _repository.GetAllAsync();
        }

        return components.Select(MapToDto).ToList();
    }

    public async Task<AttractionComponentDto> UpdateSingleAsync(Guid id, UpdateSingleAttractionDto dto)
    {
        var component = await _repository.GetByIdAsync(id)
            ?? throw new InvalidOperationException($"Attraction {id} not found.");

        if (component is not SingleAttraction attraction)
            throw new InvalidOperationException("Cannot update a group with single attraction data.");

        attraction.Update(
            dto.Name,
            dto.Description,
            new Category(dto.Category),
            new Location(dto.City, dto.Address, dto.Latitude, dto.Longitude),
            dto.OpeningTime.HasValue && dto.ClosingTime.HasValue
                ? new OpeningHours(dto.OpeningTime.Value, dto.ClosingTime.Value)
                : null,
            new SeasonalAvailability(dto.AvailableSpring, dto.AvailableSummer, dto.AvailableAutumn, dto.AvailableWinter));

        await _repository.UpdateAsync(attraction);
        return MapToDto(attraction);
    }

    public async Task<AttractionComponentDto> PublishAsync(Guid id)
    {
        var component = await _repository.GetByIdAsync(id)
            ?? throw new InvalidOperationException($"Attraction {id} not found.");

        component.Publish();
        await _repository.UpdateAsync(component);
        return MapToDto(component);
    }

    public async Task<AttractionComponentDto> ArchiveAsync(Guid id)
    {
        var component = await _repository.GetByIdAsync(id)
            ?? throw new InvalidOperationException($"Attraction {id} not found.");

        component.Archive();
        await _repository.UpdateAsync(component);
        return MapToDto(component);
    }

    private static AttractionComponentDto MapToDto(AttractionComponent component)
    {
        return component switch
        {
            SingleAttraction single => new AttractionComponentDto(
                single.Id,
                single.Name,
                single.Description,
                single.Status.ToString(),
                "Single",
                single.Category.Value,
                single.Location.City,
                single.Location.Address,
                single.Location.Latitude,
                single.Location.Longitude,
                single.OpeningHours?.Open,
                single.OpeningHours?.Close,
                single.SeasonalAvailability.Spring,
                single.SeasonalAvailability.Summer,
                single.SeasonalAvailability.Autumn,
                single.SeasonalAvailability.Winter,
                null),
            AttractionGroup group => new AttractionComponentDto(
                group.Id,
                group.Name,
                group.Description,
                group.Status.ToString(),
                "Group",
                null, null, null, null, null, null, null, null, null, null, null,
                group.Children.Select(MapToDto).ToList()),
            _ => throw new InvalidOperationException("Unknown component type.")
        };
    }
}

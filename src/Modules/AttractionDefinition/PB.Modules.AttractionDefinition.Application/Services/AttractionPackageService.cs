using PB.Shared.Domain;
using PB.Modules.AttractionDefinition.Application.DTOs;
using PB.Modules.AttractionDefinition.Domain.Aggregates;
using PB.Modules.AttractionDefinition.Domain.Ports;
using PB.Modules.AttractionDefinition.Domain.ValueObjects;
using PB.Modules.AttractionDefinition.Domain.Enums;

namespace PB.Modules.AttractionDefinition.Application.Services;

public class AttractionPackageService : IAttractionPackageService
{
    private readonly IAttractionComponentRepository _repository;

    public AttractionPackageService(IAttractionComponentRepository repository)
    {
        _repository = repository;
    }

    public async Task<AttractionPackageDto> CreateAsync(CreatePackageDto dto)
    {
        var rule = MapSelectionRule(dto.SelectionRule);
        var package = new AttractionPackage(dto.Name, dto.Description, rule);

        if (dto.ComponentIds != null)
            foreach (var id in dto.ComponentIds)
                package.AddComponent(id);

        await _repository.AddAsync(package);
        return MapToDto(package);
    }

    public async Task<AttractionPackageDto?> GetByIdAsync(Guid id)
    {
        var component = await _repository.GetByIdAsync(id);
        return component is AttractionPackage package ? MapToDto(package) : null;
    }

    public async Task<IEnumerable<AttractionPackageDto>> GetAllAsync()
    {
        var all = await _repository.GetAllPackagesAsync();
        return all.Select(MapToDto);
    }

    public async Task<AttractionPackageDto> UpdateAsync(Guid id, UpdatePackageDto dto)
    {
        var package = (await _repository.GetByIdAsync(id) as AttractionPackage)
            ?? throw new DomainException($"Package {id} not found");
        var rule = MapSelectionRule(dto.SelectionRule);
        package.Update(dto.Name, dto.Description, rule);
        await _repository.UpdateAsync(package);
        return MapToDto(package);
    }

    public async Task DeleteAsync(Guid id)
    {
        var package = (await _repository.GetByIdAsync(id) as AttractionPackage)
            ?? throw new DomainException($"Package {id} not found");
        await _repository.DeleteAsync(id);
    }

    public async Task<AttractionPackageDto> AddComponentAsync(Guid id, Guid componentId)
    {
        var package = (await _repository.GetByIdAsync(id) as AttractionPackage)
            ?? throw new DomainException($"Package {id} not found");
        package.AddComponent(componentId);
        await _repository.UpdateAsync(package);
        return MapToDto(package);
    }

    public async Task<AttractionPackageDto> RemoveComponentAsync(Guid id, Guid componentId)
    {
        var package = (await _repository.GetByIdAsync(id) as AttractionPackage)
            ?? throw new DomainException($"Package {id} not found");
        package.RemoveComponent(componentId);
        await _repository.UpdateAsync(package);
        return MapToDto(package);
    }

    private static SelectionRule MapSelectionRule(SelectionRuleDto dto)
    {
        return dto.Type.ToLower() switch
        {
            "all" => SelectionRule.All(),
            "pickn" => SelectionRule.PickN(dto.Count ?? throw new DomainException("PickN requires Count")),
            _ => throw new DomainException($"Unknown selection rule type: {dto.Type}")
        };
    }

    private static SelectionRuleDto MapSelectionRuleDto(SelectionRule rule) =>
        new SelectionRuleDto(rule.Type.ToString(), rule.Count);

    private static AttractionPackageDto MapToDto(AttractionPackage p) =>
        new AttractionPackageDto(
            p.Id,
            p.Name,
            p.Description,
            MapSelectionRuleDto(p.SelectionRule),
            p.ComponentIds.ToList());
}

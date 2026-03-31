using PB.Shared.Domain;
using PB.Modules.TripSelection.Application.DTOs;
using PB.Modules.TripSelection.Domain.Aggregates;
using PB.Modules.TripSelection.Domain.Enums;
using PB.Modules.TripSelection.Domain.Ports;

namespace PB.Modules.TripSelection.Application.Services;

public class AttractionRelationService : IAttractionRelationService
{
    private readonly IAttractionRelationRepository _repository;

    public AttractionRelationService(IAttractionRelationRepository repository)
    {
        _repository = repository;
    }

    public async Task<AttractionRelationDto> CreateAsync(CreateRelationDto dto)
    {
        if (!Enum.TryParse<RelationType>(dto.Type, true, out var relationType))
            throw new DomainException($"Unknown relation type: {dto.Type}");

        var relation = new AttractionRelation(dto.SourceId, dto.TargetId, relationType, dto.Context, dto.Description);
        await _repository.AddAsync(relation);
        return MapToDto(relation);
    }

    public async Task<AttractionRelationDto?> GetByIdAsync(Guid id)
    {
        var relation = await _repository.GetByIdAsync(id);
        return relation == null ? null : MapToDto(relation);
    }

    public async Task<IEnumerable<AttractionRelationDto>> GetAllAsync()
    {
        var all = await _repository.GetAllAsync();
        return all.Select(MapToDto);
    }

    public async Task DeleteAsync(Guid id)
    {
        var relation = await _repository.GetByIdAsync(id)
            ?? throw new DomainException($"Relation {id} not found");
        await _repository.DeleteAsync(id);
    }

    private static AttractionRelationDto MapToDto(AttractionRelation r) =>
        new AttractionRelationDto(r.Id, r.SourceId, r.TargetId, r.Type.ToString(), r.Context, r.Description);
}

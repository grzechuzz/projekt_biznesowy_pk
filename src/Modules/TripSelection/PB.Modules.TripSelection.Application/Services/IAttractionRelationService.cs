using PB.Modules.TripSelection.Application.DTOs;

namespace PB.Modules.TripSelection.Application.Services;

public interface IAttractionRelationService
{
    Task<AttractionRelationDto> CreateAsync(CreateRelationDto dto);
    Task<AttractionRelationDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<AttractionRelationDto>> GetAllAsync();
    Task DeleteAsync(Guid id);
}

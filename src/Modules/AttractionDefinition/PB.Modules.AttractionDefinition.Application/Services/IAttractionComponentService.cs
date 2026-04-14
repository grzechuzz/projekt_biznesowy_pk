using PB.Modules.AttractionDefinition.Application.DTOs;

namespace PB.Modules.AttractionDefinition.Application.Services;

public interface IAttractionComponentService
{
    Task<AttractionComponentDto> CreateAsync(CreateAttractionComponentDto dto);
    Task<AttractionComponentDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<AttractionComponentDto>> GetAllAsync(string? type = null, string? tagFilter = null, string? cityFilter = null, bool? isComplete = null);
    Task<AttractionComponentDto> UpdateAsync(Guid id, UpdateAttractionComponentDto dto);
    Task DeleteAsync(Guid id);
    Task<AttractionComponentDto> AddTagAsync(Guid id, TagDto tag);
    Task<AttractionComponentDto> RemoveTagAsync(Guid id, TagDto tag);
    Task<AttractionComponentDto> AddComponentAsync(Guid id, Guid componentId);
    Task<AttractionComponentDto> RemoveComponentAsync(Guid id, Guid componentId);
}

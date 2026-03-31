using PB.Modules.AttractionDefinition.Application.DTOs;

namespace PB.Modules.AttractionDefinition.Application.Services;

public interface IAttractionDefinitionService
{
    Task<AttractionDefinitionDto> CreateAsync(CreateAttractionDefinitionDto dto);
    Task<AttractionDefinitionDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<AttractionDefinitionDto>> GetAllAsync(string? tagFilter = null, string? cityFilter = null, bool? isComplete = null);
    Task<AttractionDefinitionDto> UpdateAsync(Guid id, UpdateAttractionDefinitionDto dto);
    Task DeleteAsync(Guid id);
    Task<AttractionDefinitionDto> AddVariantAsync(Guid id, AddVariantDto dto);
    Task<AttractionDefinitionDto> UpdateVariantAsync(Guid id, Guid variantId, UpdateVariantDto dto);
    Task<AttractionDefinitionDto> RemoveVariantAsync(Guid id, Guid variantId);
    Task<AttractionDefinitionDto> AddTagAsync(Guid id, TagDto tag);
    Task<AttractionDefinitionDto> RemoveTagAsync(Guid id, TagDto tag);
}

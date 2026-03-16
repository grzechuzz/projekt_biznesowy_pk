namespace PB.Modules.AttractionDefinition.Application.Services;

using PB.Modules.AttractionDefinition.Application.DTOs;

public interface IAttractionDefinitionService
{
    Task<AttractionComponentDto> CreateSingleAsync(CreateSingleAttractionDto dto);
    Task<AttractionComponentDto> CreateGroupAsync(CreateAttractionGroupDto dto);
    Task<AttractionComponentDto?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<AttractionComponentDto>> GetAllAsync(string? statusFilter = null);
    Task<AttractionComponentDto> UpdateSingleAsync(Guid id, UpdateSingleAttractionDto dto);
    Task<AttractionComponentDto> PublishAsync(Guid id);
    Task<AttractionComponentDto> ArchiveAsync(Guid id);
}

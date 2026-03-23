using PB.Modules.AttractionDefinition.Application.DTOs;

namespace PB.Modules.AttractionDefinition.Application.Services;

public interface IAttractionPackageService
{
    Task<AttractionPackageDto> CreateAsync(CreatePackageDto dto);
    Task<AttractionPackageDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<AttractionPackageDto>> GetAllAsync();
    Task<AttractionPackageDto> UpdateAsync(Guid id, UpdatePackageDto dto);
    Task DeleteAsync(Guid id);
    Task<AttractionPackageDto> AddComponentAsync(Guid id, Guid componentId);
    Task<AttractionPackageDto> RemoveComponentAsync(Guid id, Guid componentId);
}

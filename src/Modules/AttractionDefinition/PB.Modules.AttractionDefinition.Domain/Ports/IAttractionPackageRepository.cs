using PB.Modules.AttractionDefinition.Domain.Aggregates;

namespace PB.Modules.AttractionDefinition.Domain.Ports;

public interface IAttractionPackageRepository
{
    Task<AttractionPackage?> GetByIdAsync(Guid id);
    Task<IEnumerable<AttractionPackage>> GetAllAsync();
    Task AddAsync(AttractionPackage package);
    Task UpdateAsync(AttractionPackage package);
    Task DeleteAsync(Guid id);
}

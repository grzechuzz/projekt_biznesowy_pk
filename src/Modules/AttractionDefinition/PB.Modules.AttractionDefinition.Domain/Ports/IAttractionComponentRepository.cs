using PB.Shared.Domain;
using PB.Modules.AttractionDefinition.Domain.Aggregates;
using AttractionDefinitionAggregate = PB.Modules.AttractionDefinition.Domain.Aggregates.AttractionDefinition;

namespace PB.Modules.AttractionDefinition.Domain.Ports;

public interface IAttractionComponentRepository
{
    Task<AttractionComponent?> GetByIdAsync(Guid id);
    Task<IEnumerable<AttractionComponent>> GetAllAsync();
    Task<IEnumerable<AttractionDefinitionAggregate>> GetAllDefinitionsAsync();
    Task<IEnumerable<AttractionDefinitionAggregate>> GetDefinitionsByTagAsync(Tag tag);
    Task<IEnumerable<AttractionPackage>> GetAllPackagesAsync();
    Task AddAsync(AttractionComponent component);
    Task UpdateAsync(AttractionComponent component);
    Task DeleteAsync(Guid id);
}

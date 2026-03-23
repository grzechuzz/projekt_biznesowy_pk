using PB.Shared.Domain;
using AttractionDefinitionAggregate = PB.Modules.AttractionDefinition.Domain.Aggregates.AttractionDefinition;

namespace PB.Modules.AttractionDefinition.Domain.Ports;

public interface IAttractionDefinitionRepository
{
    Task<AttractionDefinitionAggregate?> GetByIdAsync(Guid id);
    Task<IEnumerable<AttractionDefinitionAggregate>> GetAllAsync();
    Task<IEnumerable<AttractionDefinitionAggregate>> GetByTagAsync(Tag tag);
    Task AddAsync(AttractionDefinitionAggregate definition);
    Task UpdateAsync(AttractionDefinitionAggregate definition);
    Task DeleteAsync(Guid id);
}

using PB.Shared.Domain;
using PB.Modules.AttractionDefinition.Domain.Aggregates;

namespace PB.Modules.AttractionDefinition.Domain.Ports;

public interface IAttractionComponentRepository
{
    Task<AttractionComponent?> GetByIdAsync(Guid id);
    Task<IEnumerable<AttractionComponent>> GetAllAsync();
    Task AddAsync(AttractionComponent component);
    Task UpdateAsync(AttractionComponent component);
    Task DeleteAsync(Guid id);
}

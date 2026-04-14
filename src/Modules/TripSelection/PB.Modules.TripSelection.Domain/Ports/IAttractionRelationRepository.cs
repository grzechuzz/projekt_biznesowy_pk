using PB.Modules.TripSelection.Domain.Aggregates;

namespace PB.Modules.TripSelection.Domain.Ports;

public interface IAttractionRelationRepository
{
    Task<AttractionRelation?> GetByIdAsync(Guid id);
    Task<IEnumerable<AttractionRelation>> GetAllAsync();
    Task<IEnumerable<AttractionRelation>> GetBySourceComponentIdAsync(Guid sourceComponentId);
    Task AddAsync(AttractionRelation relation);
    Task DeleteAsync(Guid id);
}

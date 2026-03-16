namespace PB.Modules.AttractionDefinition.Domain.Repositories;

using PB.Modules.AttractionDefinition.Domain.Entities;

public interface IAttractionComponentRepository
{
    Task<AttractionComponent?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<AttractionComponent>> GetAllAsync();
    Task<IReadOnlyList<AttractionComponent>> GetByStatusAsync(AttractionStatus status);
    Task AddAsync(AttractionComponent component);
    Task UpdateAsync(AttractionComponent component);
}

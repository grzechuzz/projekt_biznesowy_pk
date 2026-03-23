using PB.Modules.Availability.Domain.Aggregates;

namespace PB.Modules.Availability.Domain.Ports;

public interface ITicketPoolRepository
{
    Task<TicketPool?> GetByIdAsync(Guid id);
    Task<TicketPool?> GetByCatalogEntryIdAsync(Guid catalogEntryId);
    Task<IEnumerable<TicketPool>> GetAllAsync();
    Task AddAsync(TicketPool pool);
    Task UpdateAsync(TicketPool pool);
    Task DeleteAsync(Guid id);
}

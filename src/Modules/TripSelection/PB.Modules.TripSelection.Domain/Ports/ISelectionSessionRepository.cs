using PB.Modules.TripSelection.Domain.Aggregates;

namespace PB.Modules.TripSelection.Domain.Ports;

public interface ISelectionSessionRepository
{
    Task<SelectionSession?> GetByIdAsync(Guid id);
    Task AddAsync(SelectionSession session);
    Task UpdateAsync(SelectionSession session);
}

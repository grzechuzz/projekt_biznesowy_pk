namespace PB.Modules.TripSelection.Domain.Repositories;

using PB.Modules.TripSelection.Domain.Entities;

public interface ITripSelectionResultRepository
{
    Task<TripSelectionResult?> GetByIdAsync(Guid id);
    Task AddAsync(TripSelectionResult result);
}

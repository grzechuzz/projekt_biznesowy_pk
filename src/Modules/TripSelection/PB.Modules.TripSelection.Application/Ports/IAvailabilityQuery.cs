namespace PB.Modules.TripSelection.Application.Ports;

public interface IAvailabilityQuery
{
    Task<bool> IsAvailableAsync(Guid catalogEntryId);
    Task<int> GetAvailableCountAsync(Guid catalogEntryId);
}

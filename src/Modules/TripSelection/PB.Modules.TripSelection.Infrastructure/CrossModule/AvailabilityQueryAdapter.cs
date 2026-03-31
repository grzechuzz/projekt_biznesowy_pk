using PB.Modules.TripSelection.Application.Ports;
using PB.Modules.Availability.Application.Services;

namespace PB.Modules.TripSelection.Infrastructure.CrossModule;

public class AvailabilityQueryAdapter : IAvailabilityQuery
{
    private readonly IAvailabilityService _availabilityService;

    public AvailabilityQueryAdapter(IAvailabilityService availabilityService)
    {
        _availabilityService = availabilityService;
    }

    public async Task<bool> IsAvailableAsync(Guid catalogEntryId)
    {
        var dto = await _availabilityService.CheckAvailabilityAsync(catalogEntryId);
        return dto.IsAvailable;
    }

    public async Task<int> GetAvailableCountAsync(Guid catalogEntryId)
    {
        var dto = await _availabilityService.CheckAvailabilityAsync(catalogEntryId);
        return dto.AvailableCount;
    }
}

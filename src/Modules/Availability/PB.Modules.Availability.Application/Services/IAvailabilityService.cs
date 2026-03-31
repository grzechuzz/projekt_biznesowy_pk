using PB.Modules.Availability.Application.DTOs;

namespace PB.Modules.Availability.Application.Services;

public interface IAvailabilityService
{
    Task<TicketPoolDto> CreatePoolAsync(CreateTicketPoolDto dto);
    Task<TicketPoolDto?> GetPoolAsync(Guid id);
    Task<TicketPoolDto?> GetPoolByCatalogEntryAsync(Guid catalogEntryId);
    Task<IEnumerable<TicketPoolDto>> GetAllPoolsAsync();
    Task<ReservationDto> ReserveAsync(Guid poolId, ReserveTicketsDto dto);
    Task<ReservationDto> ConfirmReservationAsync(Guid poolId, Guid reservationId);
    Task CancelReservationAsync(Guid poolId, Guid reservationId);
    Task<int> ExpireOutdatedAsync(Guid poolId);
    Task<TicketPoolDto> IncreaseCapacityAsync(Guid poolId, ChangeCapacityDto dto);
    Task<TicketPoolDto> ReduceCapacityAsync(Guid poolId, ChangeCapacityDto dto);
    Task<AvailabilityDto> CheckAvailabilityAsync(Guid catalogEntryId);
    Task DeletePoolAsync(Guid id);
}

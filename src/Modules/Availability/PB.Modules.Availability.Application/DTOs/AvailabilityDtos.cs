namespace PB.Modules.Availability.Application.DTOs;

public record ReservationDto(Guid Id, int Quantity, string Status, DateTime CreatedAt, DateTime? ExpiresAt, string? Notes);

public record TicketPoolDto(
    Guid Id,
    Guid CatalogEntryId,
    int TotalCapacity,
    int PendingCount,
    int ConfirmedCount,
    int AvailableCount,
    bool IsAvailable,
    List<ReservationDto> Reservations);

public record AvailabilityDto(Guid CatalogEntryId, bool IsAvailable, int AvailableCount, int TotalCapacity);

public record CreateTicketPoolDto(Guid CatalogEntryId, int TotalCapacity);

public record ReserveTicketsDto(int Quantity, DateTime? ExpiresAt, string? Notes);

public record ChangeCapacityDto(int Amount);

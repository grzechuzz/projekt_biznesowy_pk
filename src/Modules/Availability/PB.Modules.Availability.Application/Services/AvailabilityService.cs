using PB.Shared.Domain;
using PB.Modules.Availability.Application.DTOs;
using PB.Modules.Availability.Domain.Aggregates;
using PB.Modules.Availability.Domain.Ports;

namespace PB.Modules.Availability.Application.Services;

public class AvailabilityService : IAvailabilityService
{
    private readonly ITicketPoolRepository _repository;

    public AvailabilityService(ITicketPoolRepository repository)
    {
        _repository = repository;
    }

    public async Task<TicketPoolDto> CreatePoolAsync(CreateTicketPoolDto dto)
    {
        var pool = new TicketPool(dto.CatalogEntryId, dto.VariantId, dto.TotalCapacity);
        await _repository.AddAsync(pool);
        return MapToDto(pool);
    }

    public async Task<TicketPoolDto?> GetPoolAsync(Guid id)
    {
        var pool = await _repository.GetByIdAsync(id);
        return pool == null ? null : MapToDto(pool);
    }

    public async Task<TicketPoolDto?> GetPoolByCatalogEntryAsync(Guid catalogEntryId)
    {
        var pool = await _repository.GetByCatalogEntryIdAsync(catalogEntryId);
        return pool == null ? null : MapToDto(pool);
    }

    public async Task<IEnumerable<TicketPoolDto>> GetAllPoolsAsync()
    {
        var all = await _repository.GetAllAsync();
        return all.Select(MapToDto);
    }

    public async Task<ReservationDto> ReserveAsync(Guid poolId, ReserveTicketsDto dto)
    {
        var pool = await _repository.GetByIdAsync(poolId)
            ?? throw new DomainException($"TicketPool {poolId} not found");
        var reservation = pool.Reserve(dto.Quantity, dto.ExpiresAt, dto.Notes);
        await _repository.UpdateAsync(pool);
        return MapReservationDto(reservation);
    }

    public async Task<ReservationDto> ConfirmReservationAsync(Guid poolId, Guid reservationId)
    {
        var pool = await _repository.GetByIdAsync(poolId)
            ?? throw new DomainException($"TicketPool {poolId} not found");
        pool.ConfirmReservation(reservationId);
        await _repository.UpdateAsync(pool);
        var reservation = pool.Reservations.First(r => r.Id == reservationId);
        return MapReservationDto(reservation);
    }

    public async Task CancelReservationAsync(Guid poolId, Guid reservationId)
    {
        var pool = await _repository.GetByIdAsync(poolId)
            ?? throw new DomainException($"TicketPool {poolId} not found");
        pool.CancelReservation(reservationId);
        await _repository.UpdateAsync(pool);
    }

    public async Task<int> ExpireOutdatedAsync(Guid poolId)
    {
        var pool = await _repository.GetByIdAsync(poolId)
            ?? throw new DomainException($"TicketPool {poolId} not found");
        var count = pool.ExpireOutdated(DateTime.UtcNow);
        await _repository.UpdateAsync(pool);
        return count;
    }

    public async Task<TicketPoolDto> IncreaseCapacityAsync(Guid poolId, ChangeCapacityDto dto)
    {
        var pool = await _repository.GetByIdAsync(poolId)
            ?? throw new DomainException($"TicketPool {poolId} not found");
        pool.IncreaseCapacity(dto.Amount);
        await _repository.UpdateAsync(pool);
        return MapToDto(pool);
    }

    public async Task<TicketPoolDto> ReduceCapacityAsync(Guid poolId, ChangeCapacityDto dto)
    {
        var pool = await _repository.GetByIdAsync(poolId)
            ?? throw new DomainException($"TicketPool {poolId} not found");
        pool.ReduceCapacity(dto.Amount);
        await _repository.UpdateAsync(pool);
        return MapToDto(pool);
    }

    public async Task<AvailabilityDto> CheckAvailabilityAsync(Guid catalogEntryId)
    {
        var pool = await _repository.GetByCatalogEntryIdAsync(catalogEntryId);
        if (pool == null)
            return new AvailabilityDto(catalogEntryId, false, 0, 0);
        return new AvailabilityDto(catalogEntryId, pool.IsAvailable, pool.AvailableCount, pool.TotalCapacity);
    }

    public async Task DeletePoolAsync(Guid id)
    {
        var pool = await _repository.GetByIdAsync(id)
            ?? throw new DomainException($"TicketPool {id} not found");
        await _repository.DeleteAsync(id);
    }

    private static ReservationDto MapReservationDto(Domain.Entities.Reservation r) =>
        new ReservationDto(r.Id, r.Quantity, r.Status.ToString(), r.CreatedAt, r.ExpiresAt, r.Notes);

    private static TicketPoolDto MapToDto(TicketPool pool) =>
        new TicketPoolDto(
            pool.Id,
            pool.CatalogEntryId,
            pool.VariantId,
            pool.TotalCapacity,
            pool.PendingCount,
            pool.ConfirmedCount,
            pool.AvailableCount,
            pool.IsAvailable,
            pool.Reservations.Select(MapReservationDto).ToList());
}

using PB.Shared.Domain;
using PB.Modules.Availability.Domain.Entities;
using PB.Modules.Availability.Domain.Enums;

namespace PB.Modules.Availability.Domain.Aggregates;

public class TicketPool : AggregateRoot
{
    public Guid CatalogEntryId { get; }
    public Guid? VariantId { get; }
    public int TotalCapacity { get; private set; }
    private readonly List<Reservation> _reservations = new();

    public IReadOnlyList<Reservation> Reservations => _reservations.AsReadOnly();

    public int PendingCount => _reservations.Where(r => r.Status == ReservationStatus.Pending).Sum(r => r.Quantity);
    public int ConfirmedCount => _reservations.Where(r => r.Status == ReservationStatus.Confirmed).Sum(r => r.Quantity);
    public int AvailableCount => TotalCapacity - PendingCount - ConfirmedCount;
    public bool IsAvailable => AvailableCount > 0;

    public TicketPool(Guid catalogEntryId, Guid? variantId, int totalCapacity)
    {
        if (catalogEntryId == Guid.Empty) throw new DomainException("CatalogEntryId cannot be empty");
        if (totalCapacity <= 0) throw new DomainException("Total capacity must be positive");
        CatalogEntryId = catalogEntryId;
        VariantId = variantId;
        TotalCapacity = totalCapacity;
    }

    public Reservation Reserve(int quantity, DateTime? expiresAt = null, string? notes = null)
    {
        if (quantity <= 0) throw new DomainException("Quantity must be positive");
        if (quantity > AvailableCount)
            throw new DomainException($"Not enough capacity. Available: {AvailableCount}, requested: {quantity}");
        var reservation = new Reservation(quantity, expiresAt, notes);
        _reservations.Add(reservation);
        return reservation;
    }

    public void ConfirmReservation(Guid reservationId)
    {
        var reservation = _reservations.FirstOrDefault(r => r.Id == reservationId)
            ?? throw new DomainException($"Reservation {reservationId} not found");
        reservation.Confirm();
    }

    public void CancelReservation(Guid reservationId)
    {
        var reservation = _reservations.FirstOrDefault(r => r.Id == reservationId)
            ?? throw new DomainException($"Reservation {reservationId} not found");
        reservation.Cancel();
    }

    public int ExpireOutdated(DateTime now)
    {
        var expired = _reservations
            .Where(r => r.Status == ReservationStatus.Pending && r.ExpiresAt.HasValue && r.ExpiresAt.Value < now)
            .ToList();
        foreach (var r in expired) r.Expire();
        return expired.Count;
    }

    public void IncreaseCapacity(int amount)
    {
        if (amount <= 0) throw new DomainException("Amount must be positive");
        TotalCapacity += amount;
    }

    public void ReduceCapacity(int amount)
    {
        if (amount <= 0) throw new DomainException("Amount must be positive");
        var newCapacity = TotalCapacity - amount;
        if (newCapacity < ConfirmedCount + PendingCount)
            throw new DomainException($"Cannot reduce capacity below current reservations ({ConfirmedCount + PendingCount})");
        TotalCapacity = newCapacity;
    }
}

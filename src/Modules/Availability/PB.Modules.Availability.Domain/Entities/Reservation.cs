using PB.Shared.Domain;
using PB.Modules.Availability.Domain.Enums;

namespace PB.Modules.Availability.Domain.Entities;

public class Reservation : Entity
{
    public int Quantity { get; }
    public ReservationStatus Status { get; private set; }
    public DateTime CreatedAt { get; }
    public DateTime? ExpiresAt { get; }
    public string? Notes { get; }

    public Reservation(int quantity, DateTime? expiresAt = null, string? notes = null)
    {
        if (quantity <= 0) throw new DomainException("Quantity must be positive");
        Quantity = quantity;
        Status = ReservationStatus.Pending;
        CreatedAt = DateTime.UtcNow;
        ExpiresAt = expiresAt;
        Notes = notes?.Trim();
    }

    public void Confirm()
    {
        if (Status != ReservationStatus.Pending) throw new DomainException($"Cannot confirm reservation in status {Status}");
        Status = ReservationStatus.Confirmed;
    }

    public void Cancel()
    {
        if (Status == ReservationStatus.Cancelled) throw new DomainException("Reservation is already cancelled");
        if (Status == ReservationStatus.Expired) throw new DomainException("Cannot cancel an expired reservation");
        Status = ReservationStatus.Cancelled;
    }

    public void Expire()
    {
        if (Status != ReservationStatus.Pending) throw new DomainException($"Cannot expire reservation in status {Status}");
        Status = ReservationStatus.Expired;
    }

    public bool IsActive => Status == ReservationStatus.Pending || Status == ReservationStatus.Confirmed;
}

namespace TransportRental.Core.Models;

public enum RentalStatus
{
    Active,
    Completed,
    Overdue,
    Cancelled
}

public class Rental
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public int CustomerId { get; set; }
    public int UserId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime ExpectedEndDate { get; set; }
    public decimal? TotalPrice { get; set; }
    public RentalStatus Status { get; set; } = RentalStatus.Active;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Vehicle Vehicle { get; set; } = null!;
    public Customer Customer { get; set; } = null!;
    public User User { get; set; } = null!;
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

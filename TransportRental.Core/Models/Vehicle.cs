namespace TransportRental.Core.Models;

public class Vehicle
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public string? LicensePlate { get; set; }
    public decimal HourlyRate { get; set; }
    public decimal DailyRate { get; set; }
    public bool IsAvailable { get; set; } = true;
    public int? Year { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public VehicleCategory Category { get; set; } = null!;
    public ICollection<Rental> Rentals { get; set; } = new List<Rental>();
}

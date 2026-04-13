namespace TransportRental.Core.Models;

public class Customer
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string PassportNumber { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string? Address { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string FullName => $"{LastName} {FirstName}" + (MiddleName != null ? $" {MiddleName}" : "");

    public ICollection<Rental> Rentals { get; set; } = new List<Rental>();
}

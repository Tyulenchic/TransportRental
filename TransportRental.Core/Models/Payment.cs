namespace TransportRental.Core.Models;

public enum PaymentMethod
{
    Cash,
    Card,
    Transfer
}

public class Payment
{
    public int Id { get; set; }
    public int RentalId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public PaymentMethod Method { get; set; }

    public Rental Rental { get; set; } = null!;
}

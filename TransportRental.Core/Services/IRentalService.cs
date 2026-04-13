using TransportRental.Core.Models;

namespace TransportRental.Core.Services;

public interface IRentalService
{
    Task<List<Rental>> GetAllAsync();
    Task<List<Rental>> GetActiveAsync();
    Task<List<Rental>> GetOverdueAsync();
    Task<List<Rental>> SearchAsync(RentalStatus? status, DateTime? from, DateTime? to);
    Task<Rental?> GetByIdAsync(int id);
    Task<Rental> CreateRentalAsync(int vehicleId, int customerId, int userId, DateTime expectedEndDate, string? notes);
    Task<Rental> ReturnVehicleAsync(int rentalId, PaymentMethod paymentMethod);
    Task CancelRentalAsync(int rentalId);
    decimal CalculatePrice(Vehicle vehicle, DateTime start, DateTime end);
}

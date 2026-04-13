using Microsoft.EntityFrameworkCore;
using TransportRental.Core.Data;
using TransportRental.Core.Models;

namespace TransportRental.Core.Services;

public class RentalService : IRentalService
{
    private readonly AppDbContext _context;

    public RentalService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Rental>> GetAllAsync()
        => await _context.Rentals
            .Include(r => r.Vehicle).ThenInclude(v => v.Category)
            .Include(r => r.Customer)
            .Include(r => r.User)
            .Include(r => r.Payments)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

    public async Task<List<Rental>> GetActiveAsync()
        => await _context.Rentals
            .Include(r => r.Vehicle)
            .Include(r => r.Customer)
            .Include(r => r.User)
            .Where(r => r.Status == RentalStatus.Active || r.Status == RentalStatus.Overdue)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

    public async Task<List<Rental>> GetOverdueAsync()
    {
        // Update overdue statuses first
        var overdueRentals = await _context.Rentals
            .Where(r => r.Status == RentalStatus.Active && r.ExpectedEndDate < DateTime.UtcNow)
            .ToListAsync();

        foreach (var r in overdueRentals)
            r.Status = RentalStatus.Overdue;

        if (overdueRentals.Any())
            await _context.SaveChangesAsync();

        return await _context.Rentals
            .Include(r => r.Vehicle)
            .Include(r => r.Customer)
            .Include(r => r.User)
            .Where(r => r.Status == RentalStatus.Overdue)
            .OrderBy(r => r.ExpectedEndDate)
            .ToListAsync();
    }

    public async Task<List<Rental>> SearchAsync(RentalStatus? status, DateTime? from, DateTime? to)
    {
        var q = _context.Rentals
            .Include(r => r.Vehicle)
            .Include(r => r.Customer)
            .Include(r => r.User)
            .AsQueryable();

        if (status.HasValue)
            q = q.Where(r => r.Status == status.Value);

        if (from.HasValue)
            q = q.Where(r => r.StartDate >= from.Value);

        if (to.HasValue)
            q = q.Where(r => r.StartDate <= to.Value);

        return await q.OrderByDescending(r => r.CreatedAt).ToListAsync();
    }

    public async Task<Rental?> GetByIdAsync(int id)
        => await _context.Rentals
            .Include(r => r.Vehicle).ThenInclude(v => v.Category)
            .Include(r => r.Customer)
            .Include(r => r.User)
            .Include(r => r.Payments)
            .FirstOrDefaultAsync(r => r.Id == id);

    public async Task<Rental> CreateRentalAsync(int vehicleId, int customerId, int userId, DateTime expectedEndDate, string? notes)
    {
        var vehicle = await _context.Vehicles.FindAsync(vehicleId);
        if (vehicle == null) throw new InvalidOperationException("Транспорт не найден");
        if (!vehicle.IsAvailable) throw new InvalidOperationException("Транспорт недоступен для проката");

        var rental = new Rental
        {
            VehicleId = vehicleId,
            CustomerId = customerId,
            UserId = userId,
            StartDate = DateTime.UtcNow,
            ExpectedEndDate = expectedEndDate,
            Status = RentalStatus.Active,
            Notes = notes
        };

        vehicle.IsAvailable = false;
        _context.Rentals.Add(rental);
        await _context.SaveChangesAsync();

        await _context.Entry(rental).Reference(r => r.Vehicle).LoadAsync();
        await _context.Entry(rental).Reference(r => r.Customer).LoadAsync();
        await _context.Entry(rental).Reference(r => r.User).LoadAsync();
        return rental;
    }

    public async Task<Rental> ReturnVehicleAsync(int rentalId, PaymentMethod paymentMethod)
    {
        var rental = await _context.Rentals
            .Include(r => r.Vehicle)
            .Include(r => r.Customer)
            .FirstOrDefaultAsync(r => r.Id == rentalId);

        if (rental == null) throw new InvalidOperationException("Прокат не найден");
        if (rental.Status == RentalStatus.Completed) throw new InvalidOperationException("Прокат уже завершён");

        rental.EndDate = DateTime.UtcNow;
        rental.TotalPrice = CalculatePrice(rental.Vehicle, rental.StartDate, rental.EndDate.Value);
        rental.Status = RentalStatus.Completed;
        rental.Vehicle.IsAvailable = true;

        var payment = new Payment
        {
            RentalId = rentalId,
            Amount = rental.TotalPrice.Value,
            Method = paymentMethod,
            PaymentDate = DateTime.UtcNow
        };
        _context.Payments.Add(payment);

        await _context.SaveChangesAsync();
        return rental;
    }

    public async Task CancelRentalAsync(int rentalId)
    {
        var rental = await _context.Rentals.Include(r => r.Vehicle).FirstOrDefaultAsync(r => r.Id == rentalId);
        if (rental == null) throw new InvalidOperationException("Прокат не найден");
        if (rental.Status == RentalStatus.Completed) throw new InvalidOperationException("Невозможно отменить завершённый прокат");

        rental.Status = RentalStatus.Cancelled;
        rental.EndDate = DateTime.UtcNow;
        rental.Vehicle.IsAvailable = true;
        await _context.SaveChangesAsync();
    }

    public decimal CalculatePrice(Vehicle vehicle, DateTime start, DateTime end)
    {
        var duration = end - start;
        if (duration.TotalHours <= 24)
        {
            var hours = Math.Max(1, (int)Math.Ceiling(duration.TotalHours));
            return hours * vehicle.HourlyRate;
        }
        else
        {
            var days = (int)Math.Ceiling(duration.TotalDays);
            return days * vehicle.DailyRate;
        }
    }
}

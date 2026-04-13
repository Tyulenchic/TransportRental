using Microsoft.EntityFrameworkCore;
using TransportRental.Core.Data;
using TransportRental.Core.Models;

namespace TransportRental.Core.Services;

public class VehicleService : IVehicleService
{
    private readonly AppDbContext _context;

    public VehicleService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Vehicle>> GetAllAsync()
        => await _context.Vehicles.Include(v => v.Category).OrderBy(v => v.Name).ToListAsync();

    public async Task<List<Vehicle>> GetAvailableAsync()
        => await _context.Vehicles.Include(v => v.Category).Where(v => v.IsAvailable).OrderBy(v => v.Name).ToListAsync();

    public async Task<List<Vehicle>> SearchAsync(string? query, int? categoryId, bool? availableOnly)
    {
        var q = _context.Vehicles.Include(v => v.Category).AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
            q = q.Where(v => v.Name.ToLower().Contains(query.ToLower()) ||
                             (v.LicensePlate != null && v.LicensePlate.ToLower().Contains(query.ToLower())));

        if (categoryId.HasValue)
            q = q.Where(v => v.CategoryId == categoryId.Value);

        if (availableOnly == true)
            q = q.Where(v => v.IsAvailable);

        return await q.OrderBy(v => v.Name).ToListAsync();
    }

    public async Task<Vehicle?> GetByIdAsync(int id)
        => await _context.Vehicles.Include(v => v.Category).FirstOrDefaultAsync(v => v.Id == id);

    public async Task<Vehicle> CreateAsync(Vehicle vehicle)
    {
        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();
        await _context.Entry(vehicle).Reference(v => v.Category).LoadAsync();
        return vehicle;
    }

    public async Task UpdateAsync(Vehicle vehicle)
    {
        _context.Vehicles.Update(vehicle);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var vehicle = await _context.Vehicles.FindAsync(id);
        if (vehicle == null) throw new InvalidOperationException("Транспорт не найден");
        if (await _context.Rentals.AnyAsync(r => r.VehicleId == id && r.Status == RentalStatus.Active))
            throw new InvalidOperationException("Невозможно удалить транспорт с активным прокатом");
        _context.Vehicles.Remove(vehicle);
        await _context.SaveChangesAsync();
    }

    public async Task<List<VehicleCategory>> GetCategoriesAsync()
        => await _context.VehicleCategories.OrderBy(c => c.Name).ToListAsync();

    public async Task<VehicleCategory> CreateCategoryAsync(VehicleCategory category)
    {
        _context.VehicleCategories.Add(category);
        await _context.SaveChangesAsync();
        return category;
    }

    public async Task UpdateCategoryAsync(VehicleCategory category)
    {
        _context.VehicleCategories.Update(category);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteCategoryAsync(int id)
    {
        var category = await _context.VehicleCategories.FindAsync(id);
        if (category == null) throw new InvalidOperationException("Категория не найдена");
        if (await _context.Vehicles.AnyAsync(v => v.CategoryId == id))
            throw new InvalidOperationException("Невозможно удалить категорию с привязанным транспортом");
        _context.VehicleCategories.Remove(category);
        await _context.SaveChangesAsync();
    }
}

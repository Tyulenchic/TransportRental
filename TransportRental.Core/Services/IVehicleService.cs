using TransportRental.Core.Models;

namespace TransportRental.Core.Services;

public interface IVehicleService
{
    Task<List<Vehicle>> GetAllAsync();
    Task<List<Vehicle>> GetAvailableAsync();
    Task<List<Vehicle>> SearchAsync(string? query, int? categoryId, bool? availableOnly);
    Task<Vehicle?> GetByIdAsync(int id);
    Task<Vehicle> CreateAsync(Vehicle vehicle);
    Task UpdateAsync(Vehicle vehicle);
    Task DeleteAsync(int id);
    Task<List<VehicleCategory>> GetCategoriesAsync();
    Task<VehicleCategory> CreateCategoryAsync(VehicleCategory category);
    Task UpdateCategoryAsync(VehicleCategory category);
    Task DeleteCategoryAsync(int id);
}

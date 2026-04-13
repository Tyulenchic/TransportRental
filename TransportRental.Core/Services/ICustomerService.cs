using TransportRental.Core.Models;

namespace TransportRental.Core.Services;

public interface ICustomerService
{
    Task<List<Customer>> GetAllAsync();
    Task<List<Customer>> SearchAsync(string query);
    Task<Customer?> GetByIdAsync(int id);
    Task<Customer> CreateAsync(Customer customer);
    Task UpdateAsync(Customer customer);
    Task DeleteAsync(int id);
}

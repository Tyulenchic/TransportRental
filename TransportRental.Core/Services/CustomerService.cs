using Microsoft.EntityFrameworkCore;
using TransportRental.Core.Data;
using TransportRental.Core.Models;

namespace TransportRental.Core.Services;

public class CustomerService : ICustomerService
{
    private readonly AppDbContext _context;

    public CustomerService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Customer>> GetAllAsync()
        => await _context.Customers.OrderBy(c => c.LastName).ThenBy(c => c.FirstName).ToListAsync();

    public async Task<List<Customer>> SearchAsync(string query)
    {
        var q = query.ToLower();
        return await _context.Customers
            .Where(c => c.LastName.ToLower().Contains(q) ||
                        c.FirstName.ToLower().Contains(q) ||
                        c.Phone.Contains(q) ||
                        c.PassportNumber.Contains(q))
            .OrderBy(c => c.LastName)
            .ToListAsync();
    }

    public async Task<Customer?> GetByIdAsync(int id)
        => await _context.Customers.FindAsync(id);

    public async Task<Customer> CreateAsync(Customer customer)
    {
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
        return customer;
    }

    public async Task UpdateAsync(Customer customer)
    {
        _context.Customers.Update(customer);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null) throw new InvalidOperationException("Клиент не найден");
        if (await _context.Rentals.AnyAsync(r => r.CustomerId == id && r.Status == RentalStatus.Active))
            throw new InvalidOperationException("Невозможно удалить клиента с активным прокатом");
        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();
    }
}

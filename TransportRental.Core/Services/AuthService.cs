using Microsoft.EntityFrameworkCore;
using TransportRental.Core.Data;
using TransportRental.Core.Helpers;
using TransportRental.Core.Models;

namespace TransportRental.Core.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;

    public AuthService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> LoginAsync(string username, string password)
    {
        var hash = PasswordHasher.Hash(password);
        return await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Username == username && u.PasswordHash == hash && u.IsActive);
    }

    public async Task<User> RegisterAsync(string username, string password, string fullName, int roleId)
    {
        if (await _context.Users.AnyAsync(u => u.Username == username))
            throw new InvalidOperationException("Пользователь с таким логином уже существует");

        var user = new User
        {
            Username = username,
            PasswordHash = PasswordHasher.Hash(password),
            FullName = fullName,
            RoleId = roleId,
            IsActive = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        await _context.Entry(user).Reference(u => u.Role).LoadAsync();
        return user;
    }

    public async Task<List<User>> GetAllUsersAsync()
        => await _context.Users.Include(u => u.Role).OrderBy(u => u.FullName).ToListAsync();

    public async Task<User?> GetUserByIdAsync(int id)
        => await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == id);

    public async Task UpdateUserAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task ChangePasswordAsync(int userId, string newPassword)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) throw new InvalidOperationException("Пользователь не найден");
        user.PasswordHash = PasswordHasher.Hash(newPassword);
        await _context.SaveChangesAsync();
    }
}

using TransportRental.Core.Models;

namespace TransportRental.Core.Services;

public interface IAuthService
{
    Task<User?> LoginAsync(string username, string password);
    Task<User> RegisterAsync(string username, string password, string fullName, int roleId);
    Task<List<User>> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(int id);
    Task UpdateUserAsync(User user);
    Task ChangePasswordAsync(int userId, string newPassword);
}

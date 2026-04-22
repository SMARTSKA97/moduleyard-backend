using Domain.Entities;

namespace Application.Interfaces;

public interface IUserRepository
{
    // A contract to save the secret for a specific email
    Task SaveTotpSecretAsync(string email, string secretKey, string? passwordHash = null);
    Task<User?> GetUserByEmailAsync(string email);
    Task CreateUserAsync(User user);
    Task UpdateUserAsync(User user);
}
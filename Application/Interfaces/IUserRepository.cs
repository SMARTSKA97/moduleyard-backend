using Domain.Entities;

namespace Application.Interfaces;

public interface IUserRepository
{
    // A contract to save the secret for a specific email
    Task SaveTotpSecretAsync(string email, string secretKey);
    Task<User?> GetUserByEmailAsync(string email);
}
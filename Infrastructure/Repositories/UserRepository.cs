using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Persistence.Models;
using Domain.Entities;
using System.Threading.Tasks;

namespace Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task SaveTotpSecretAsync(string email, string secretKey, string? passwordHash = null)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            user = new User 
            { 
                Email = email, 
                PasswordHash = passwordHash ?? "PENDING_SETUP",
                TotpSecret = secretKey,
                IsVerified = false
            };
            _context.Users.Add(user);
        }
        else
        {
            user.TotpSecret = secretKey;
            if (passwordHash != null) user.PasswordHash = passwordHash;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task CreateUserAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateUserAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }
}
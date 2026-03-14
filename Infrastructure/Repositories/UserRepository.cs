using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Persistence.Models; // Brings in the DbContext
using DomainUser = Domain.Entities.User; // 1. Nickname for the pure business model
using DbUser = Infrastructure.Persistence.Models.User; // 2. Nickname for the database model

namespace Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task SaveTotpSecretAsync(string email, string secretKey)
    {
        // Use the DbUser nickname when talking to EF Core
        DbUser? user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            user = new DbUser 
            { 
                Email = email, 
                PasswordHash = "PENDING_SETUP",
                TotpSecret = secretKey,
                IsVerified = false
            };
            _context.Users.Add(user);
        }
        else
        {
            user.TotpSecret = secretKey;
        }

        await _context.SaveChangesAsync();
    }

    // Explicitly return the DomainUser nickname to satisfy the Interface
    public async Task<DomainUser?> GetUserByEmailAsync(string email)
    {
        DbUser? dbUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        
        if (dbUser == null) return null;

        // Map the database model to the pure domain model
        return new DomainUser
        {
            Email = dbUser.Email,
            TotpSecret = dbUser.TotpSecret
        };
    }
}
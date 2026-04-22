using Application.Interfaces;
using Infrastructure.Persistence.Models;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Repositories;

public class PasskeyCredentialRepository : IPasskeyCredentialRepository
{
    private readonly ApplicationDbContext _context;

    public PasskeyCredentialRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PasskeyCredential?> GetByIdAsync(byte[] credentialId)
    {
        return await _context.PasskeyCredentials
            .FirstOrDefaultAsync(c => c.DescriptorId == credentialId);
    }

    public async Task<List<PasskeyCredential>> GetByUserIdAsync(Guid userId)
    {
        return await _context.PasskeyCredentials
            .Where(c => c.UserId == userId)
            .ToListAsync();
    }

    public async Task AddAsync(PasskeyCredential credential)
    {
        _context.PasskeyCredentials.Add(credential);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateCounterAsync(byte[] credentialId, uint counter)
    {
        var cred = await _context.PasskeyCredentials.FirstOrDefaultAsync(c => c.DescriptorId == credentialId);
        if (cred != null)
        {
            cred.SignatureCounter = counter;
            await _context.SaveChangesAsync();
        }
    }
}

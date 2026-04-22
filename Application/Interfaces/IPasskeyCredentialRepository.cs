using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Interfaces;

public interface IPasskeyCredentialRepository
{
    Task<PasskeyCredential?> GetByIdAsync(byte[] credentialId);
    Task<List<PasskeyCredential>> GetByUserIdAsync(Guid userId);
    Task AddAsync(PasskeyCredential credential);
    Task UpdateCounterAsync(byte[] credentialId, uint counter);
}

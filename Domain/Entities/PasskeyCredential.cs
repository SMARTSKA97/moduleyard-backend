using System;

namespace Domain.Entities;

public class PasskeyCredential
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public byte[] DescriptorId { get; set; } = null!;
    public byte[] PublicKey { get; set; } = null!;
    public byte[] UserHandle { get; set; } = null!;
    public uint SignatureCounter { get; set; }
    public string CredType { get; set; } = null!; // Let's use more descriptive names
    public DateTime RegDate { get; set; }
    public Guid AaGuid { get; set; }

    public virtual User User { get; set; } = null!;
}

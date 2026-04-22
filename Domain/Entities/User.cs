using System;
using System.Collections.Generic;

namespace Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public string? TotpSecret { get; set; }
    public bool IsVerified { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<PasskeyCredential> Credentials { get; set; } = new List<PasskeyCredential>();
}
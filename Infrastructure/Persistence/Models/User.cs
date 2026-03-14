using System;
using System.Collections.Generic;

namespace Infrastructure.Persistence.Models;

public partial class User
{
    public Guid Id { get; set; }

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? TotpSecret { get; set; }

    public bool? IsVerified { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}

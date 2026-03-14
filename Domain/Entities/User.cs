namespace Domain.Entities;

public class User
{
    public string Email { get; set; } = string.Empty;
    public string? TotpSecret { get; set; }
}
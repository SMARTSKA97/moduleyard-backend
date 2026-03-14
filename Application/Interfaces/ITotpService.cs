namespace Application.Interfaces;

public interface ITotpService
{
    // Generates a new base32 secret key (like the ones you scan with Google Authenticator)
    string GenerateSecretKey();

    // Validates the 6-digit code the user types in against their saved secret key
    bool ValidateCode(string secretKey, string code);
}
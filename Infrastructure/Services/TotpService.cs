using Application.Interfaces;
using OtpNet;

namespace Infrastructure.Services;

public class TotpService : ITotpService
{
    public string GenerateSecretKey()
    {
        // Generates a secure 20-byte secret key
        var key = KeyGeneration.GenerateRandomKey(20);
        return Base32Encoding.ToString(key);
    }

    public bool ValidateCode(string secretKey, string code)
    {
        var secretBytes = Base32Encoding.ToBytes(secretKey);
        var totp = new Totp(secretBytes);
        
        // Verifies the code with a small time window tolerance to account for slight clock delays
        return totp.VerifyTotp(code, out long timeWindowUsed, window: new VerificationWindow(previous: 1, future: 1));
    }
}
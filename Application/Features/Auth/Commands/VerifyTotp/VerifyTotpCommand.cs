using Application.Interfaces;
using MediatR;

namespace Application.Features.Auth.Commands.VerifyTotp;

// The Request: We need the email and the 6-digit code they typed in
public class VerifyTotpCommand : IRequest<bool>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}

// The Handler: Checks the code against the database
public class VerifyTotpCommandHandler : IRequestHandler<VerifyTotpCommand, bool>
{
    private readonly ITotpService _totpService;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public VerifyTotpCommandHandler(ITotpService totpService, IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _totpService = totpService;
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<bool> Handle(VerifyTotpCommand request, CancellationToken cancellationToken)
    {
        // 1. Get the user from the database
        var user = await _userRepository.GetUserByEmailAsync(request.Email);

        // 2. If user doesn't exist, fail
        if (user == null) 
        {
            Console.WriteLine($"[VerifyTotp] User not found: {request.Email}");
            return false;
        }

        // 3. Verify Password
        if (string.IsNullOrEmpty(user.PasswordHash) || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            Console.WriteLine($"[VerifyTotp] Password verification failed for: {request.Email}");
            return false;
        }

        // 4. Verify TOTP if secret exists
        if (!string.IsNullOrEmpty(user.TotpSecret))
        {
            var isValid = _totpService.ValidateCode(user.TotpSecret, request.Code);
            Console.WriteLine($"[VerifyTotp] TOTP verification for {request.Email}: {isValid} (Code: {request.Code})");
            return isValid;
        }

        Console.WriteLine($"[VerifyTotp] No TOTP secret found for user: {request.Email}. Returning true as fallback.");
        return true; 
    }
}
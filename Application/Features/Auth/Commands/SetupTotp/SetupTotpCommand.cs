using Application.Interfaces;
using MediatR;

namespace Application.Features.Auth.Commands.SetupTotp;

public class SetupTotpCommand : IRequest<string>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class SetupTotpCommandHandler : IRequestHandler<SetupTotpCommand, string>
{
    private readonly ITotpService _totpService;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public SetupTotpCommandHandler(ITotpService totpService, IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _totpService = totpService;
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    // 3. Make the method 'async' so we can await the database save
    public async Task<string> Handle(SetupTotpCommand request, CancellationToken cancellationToken)
    {
        // Generate the secure Base32 secret
        var secretKey = _totpService.GenerateSecretKey();

        // Hash the password
        var passwordHash = _passwordHasher.HashPassword(request.Password);

        // 4. SAVE TO THE NEON DATABASE!
        await _userRepository.SaveTotpSecretAsync(request.Email, secretKey, passwordHash);

        // Format the URI for the authenticator app
        var qrCodeUri = $"otpauth://totp/ModuleYard:{request.Email}?secret={secretKey}&issuer=ModuleYard";

        return qrCodeUri; // Return the string directly since we are using 'async Task'
    }
}
using Application.Interfaces;
using MediatR;

namespace Application.Features.Auth.Commands.SetupTotp;

public class SetupTotpCommand : IRequest<string>
{
    public string Email { get; set; } = string.Empty;
}

public class SetupTotpCommandHandler : IRequestHandler<SetupTotpCommand, string>
{
    private readonly ITotpService _totpService;
    private readonly IUserRepository _userRepository; // 1. Add the repository interface

    // 2. Inject it into the constructor
    public SetupTotpCommandHandler(ITotpService totpService, IUserRepository userRepository)
    {
        _totpService = totpService;
        _userRepository = userRepository;
    }

    // 3. Make the method 'async' so we can await the database save
    public async Task<string> Handle(SetupTotpCommand request, CancellationToken cancellationToken)
    {
        // Generate the secure Base32 secret
        var secretKey = _totpService.GenerateSecretKey();

        // 4. SAVE TO THE NEON DATABASE!
        await _userRepository.SaveTotpSecretAsync(request.Email, secretKey);

        // Format the URI for the authenticator app
        var qrCodeUri = $"otpauth://totp/ModuleYard:{request.Email}?secret={secretKey}&issuer=ModuleYard";

        return qrCodeUri; // Return the string directly since we are using 'async Task'
    }
}
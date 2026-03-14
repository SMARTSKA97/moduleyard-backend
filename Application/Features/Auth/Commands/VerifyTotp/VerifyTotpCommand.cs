using Application.Interfaces;
using MediatR;

namespace Application.Features.Auth.Commands.VerifyTotp;

// The Request: We need the email and the 6-digit code they typed in
public class VerifyTotpCommand : IRequest<bool>
{
    public string Email { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}

// The Handler: Checks the code against the database
public class VerifyTotpCommandHandler : IRequestHandler<VerifyTotpCommand, bool>
{
    private readonly ITotpService _totpService;
    private readonly IUserRepository _userRepository;

    public VerifyTotpCommandHandler(ITotpService totpService, IUserRepository userRepository)
    {
        _totpService = totpService;
        _userRepository = userRepository;
    }

    public async Task<bool> Handle(VerifyTotpCommand request, CancellationToken cancellationToken)
    {
        // 1. Get the user from the database
        var user = await _userRepository.GetUserByEmailAsync(request.Email);

        // 2. If user doesn't exist or hasn't set up TOTP, fail the login
        if (user == null || string.IsNullOrEmpty(user.TotpSecret))
        {
            return false;
        }

        // 3. Verify the 6-digit code against their saved secret
        var isValid = _totpService.ValidateCode(user.TotpSecret, request.Code);

        // 4. If valid, we would normally generate a JWT token here. 
        // For now, just return true/false to prove it works.
        return isValid;
    }
}
using Application.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
namespace Application.Features.Auth.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<LoginResult>;

public record LoginResult(bool Success, string Message, bool RequiresMfa = false, string? Token = null);

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public LoginCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByEmailAsync(request.Email);
        if (user == null) return new LoginResult(false, "Invalid email or password.");

        if (string.IsNullOrEmpty(user.PasswordHash) || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            return new LoginResult(false, "Invalid email or password.");

        // Check if MFA is required (e.g. if they have a TOTP secret or Passkeys)
        bool requiresMfa = !string.IsNullOrEmpty(user.TotpSecret);
        // In a real app, we'd also check for Passkeys here.
        
        return new LoginResult(true, "Login successful.", requiresMfa);
    }
}

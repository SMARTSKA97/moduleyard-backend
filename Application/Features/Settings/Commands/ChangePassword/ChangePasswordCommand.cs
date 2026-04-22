using MediatR;
using Application.Interfaces;

namespace Application.Features.Settings.Commands.ChangePassword;

public class ChangePasswordCommand : IRequest<bool>
{
    public string Email { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public ChangePasswordCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<bool> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByEmailAsync(request.Email);
        if (user == null) return false;

        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        await _userRepository.UpdateUserAsync(user);

        return true;
    }
}

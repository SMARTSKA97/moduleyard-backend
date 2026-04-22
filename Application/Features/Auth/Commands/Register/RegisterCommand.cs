using Application.Interfaces;
using Domain.Entities;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
namespace Application.Features.Auth.Commands.Register;

public record RegisterCommand(string Email, string Password) : IRequest<bool>;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<bool> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _userRepository.GetUserByEmailAsync(request.Email);
        if (existingUser != null) return false;

        var passwordHash = _passwordHasher.HashPassword(request.Password);
        
        var newUser = new User
        {
            Email = request.Email,
            PasswordHash = passwordHash,
            IsVerified = true
        };

        await _userRepository.CreateUserAsync(newUser);
        return true;
    }
}

using Application.Features.Auth.Commands.SetupTotp;
using Application.Features.Auth.Commands.VerifyTotp;
using Application.Features.Auth.Commands.Register;
using Application.Features.Auth.Commands.Login;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")] // This makes the base URL: /api/auth
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IConfiguration _configuration;

    public AuthController(IMediator mediator, IConfiguration configuration)
    {
        _mediator = mediator;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command)
    {
        var success = await _mediator.Send(command);
        if (success) return Ok(new { success = true, message = "User registered successfully." });
        return BadRequest(new { success = false, message = "Email already exists." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.Success) return Unauthorized(new { success = false, message = result.Message });

        if (result.RequiresMfa)
        {
            return Ok(new { success = true, requiresMfa = true, message = "Password verified. MFA required." });
        }

        // Generate Token if no MFA required
        var token = GenerateJwt(command.Email);
        return Ok(new { success = true, token = token, message = "Login successful." });
    }

    private string GenerateJwt(string email)
    {
        // Support both appsettings.json (JwtSettings:SecretKey) and .env (JWT_SECRET_KEY)
        var secretKey = _configuration["JWT_SECRET_KEY"] ?? _configuration["JwtSettings:SecretKey"];
        var issuer = _configuration["JWT_ISSUER"] ?? _configuration["JwtSettings:Issuer"];
        var audience = _configuration["JWT_AUDIENCE"] ?? _configuration["JwtSettings:Audience"];

        if (string.IsNullOrEmpty(secretKey))
        {
            throw new Exception("JWT Secret Key is not configured. Please check your .env file.");
        }

        var key = Encoding.UTF8.GetBytes(secretKey);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, "Admin")
            }),
            Expires = DateTime.UtcNow.AddHours(8),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    [HttpPost("setup-totp")]
    public async Task<IActionResult> SetupTotp([FromBody] SetupTotpCommand command)
    {
        // The API controller doesn't know HOW the secret is generated, 
        // it just sends the command and waits for the result.
        var qrCodeUri = await _mediator.Send(command);
        
        return Ok(new { QrCodeUri = qrCodeUri });
    }

    // [HttpPost("verify-totp")]
    // public async Task<IActionResult> VerifyTotp([FromBody] VerifyTotpCommand command)
    // {
    //     var isValid = await _mediator.Send(command);

    //     if (isValid)
    //     {
    //         return Ok(new { Message = "Login Successful! Code is valid." });
    //     }

    //     return Unauthorized(new { Message = "Invalid TOTP Code." });
    // }

    [HttpPost("verify-totp")]
    public async Task<IActionResult> VerifyTotp([FromBody] VerifyTotpCommand command)
    {
        var isValid = await _mediator.Send(command);

        if (!isValid)
        {
            return Unauthorized(new { message = "Invalid credentials or expired code." });
        }

        var token = GenerateJwt(command.Email);

        return Ok(new 
        { 
            success = true, 
            message = "Login Successful!",
            token = token 
        });
    }
}
using Application.Features.Auth.Commands.SetupTotp;
using Application.Features.Auth.Commands.VerifyTotp;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")] // This makes the base URL: /api/auth
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
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
        // 1. Your existing logic to verify the TOTP code goes here
        bool isCodeValid = VerifyYourCodeSomehow(command.Email, command.Code);

        if (!isCodeValid)
        {
            return Unauthorized(new { message = "Invalid credentials or expired code." });
        }

        // 2. Code is valid! Let's generate the JWT.
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var key = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            // Add info about the user into the token
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, request.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, "Admin") // They are an admin!
            }),
            // Token lives for 8 hours
            Expires = DateTime.UtcNow.AddHours(8), 
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var jwtString = tokenHandler.WriteToken(token);

        // 3. Return the token to Angular!
        return Ok(new 
        { 
            success = true, 
            message = "Login Successful!",
            token = jwtString // <-- Angular needs this!
        });
    }
}
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

    [HttpPost("verify-totp")]
    public async Task<IActionResult> VerifyTotp([FromBody] VerifyTotpCommand command)
    {
        var isValid = await _mediator.Send(command);

        if (isValid)
        {
            return Ok(new { Message = "Login Successful! Code is valid." });
        }

        return Unauthorized(new { Message = "Invalid TOTP Code." });
    }
}
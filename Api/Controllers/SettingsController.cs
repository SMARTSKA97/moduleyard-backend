using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Application.Features.Settings.Commands.ChangePassword;

namespace Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SettingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SettingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var email = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrEmpty(email)) return Unauthorized();

        var command = new ChangePasswordCommand { Email = email, NewPassword = request.NewPassword };
        var success = await _mediator.Send(command);

        if (success) return Ok(new { message = "Password changed successfully." });
        return BadRequest(new { message = "Could not change password." });
    }
}

public class ChangePasswordRequest
{
    public string NewPassword { get; set; } = string.Empty;
}

using Application.Interfaces;
using Fido2NetLib;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PasskeyController : ControllerBase
{
    private readonly IPasskeyService _passkeyService;
    private readonly IConfiguration _configuration;

    public PasskeyController(IPasskeyService passkeyService, IConfiguration configuration)
    {
        _passkeyService = passkeyService;
        _configuration = configuration;
    }

    [HttpPost("register-challenge")]
    public async Task<IActionResult> RegisterChallenge([FromBody] string email)
    {
        try
        {
            var options = await _passkeyService.RequestRegistrationAsync(email);
            return Ok(options);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("register-complete")]
    public async Task<IActionResult> RegisterComplete([FromQuery] string email, [FromBody] AuthenticatorAttestationRawResponse attestation)
    {
        try
        {
            await _passkeyService.RegisterAsync(email, attestation);
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("login-challenge")]
    public async Task<IActionResult> LoginChallenge([FromBody] string email)
    {
        try
        {
            var options = await _passkeyService.RequestLoginAsync(email);
            return Ok(options);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("login-complete")]
    public async Task<IActionResult> LoginComplete([FromQuery] string email, [FromBody] AuthenticatorAssertionRawResponse assertion)
    {
        try
        {
            await _passkeyService.VerifyLoginAsync(email, assertion);
            
            var token = GenerateJwt(email);
            
            return Ok(new 
            { 
                success = true, 
                message = "Login Successful with Passkey!",
                token = token 
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private string GenerateJwt(string email)
    {
        var secretKeyString = _configuration["JWT_SECRET_KEY"] ?? _configuration["JwtSettings:SecretKey"];
        var issuer = _configuration["JWT_ISSUER"] ?? _configuration["JwtSettings:Issuer"];
        var audience = _configuration["JWT_AUDIENCE"] ?? _configuration["JwtSettings:Audience"];

        if (string.IsNullOrEmpty(secretKeyString))
        {
            throw new Exception("JWT Secret Key is not configured.");
        }

        var key = Encoding.UTF8.GetBytes(secretKeyString);

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
}

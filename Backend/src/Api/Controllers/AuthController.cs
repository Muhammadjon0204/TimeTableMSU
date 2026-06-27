using Application.DTOs.AuthDTOs;
using Application.Interfaces.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/auth")]
[AllowAnonymous]
public class AuthController : ApiControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register([FromBody] RegisterDto dto)
    {
        return ToCreatedResult(await _authService.RegisterAsync(dto));
    }

    [HttpPost("login")]
    public async Task<ActionResult> Login([FromBody] LoginDto dto)
    {
        return ToOkResult(await _authService.LoginAsync(dto));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        return ToOkResult(await _authService.RefreshTokenAsync(request.RefreshToken));
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        return ToNoContentResult(await _authService.ForgotPasswordAsync(dto));
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        return ToNoContentResult(await _authService.ResetPasswordAsync(dto));
    }

    [HttpPost("external-login")]
    public async Task<ActionResult> ExternalLogin([FromBody] ExternalLoginDto dto)
    {
        return ToOkResult(await _authService.ExternalLoginAsync(dto));
    }
}

public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = null!;
}

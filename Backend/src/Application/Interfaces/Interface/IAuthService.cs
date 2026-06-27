// src/Application/Interfaces/Interface/IAuthService.cs
using Application.Common;
using Application.DTOs.AuthDTOs;

namespace Application.Interfaces.Interface;

public interface IAuthService
{
    Task<Result<AuthResultDto>> RegisterAsync(RegisterDto dto);
    Task<Result<AuthResultDto>> LoginAsync(LoginDto dto);
    Task<Result<AuthResultDto>> RefreshTokenAsync(string refreshToken);
    Task<Result> ForgotPasswordAsync(ForgotPasswordDto dto);
    Task<Result> ResetPasswordAsync(ResetPasswordDto dto);
    Task<Result<AuthResultDto>> ExternalLoginAsync(ExternalLoginDto dto);
}

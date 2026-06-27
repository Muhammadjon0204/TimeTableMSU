namespace Application.DTOs.AuthDTOs;

public class AuthResultDto
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public DateTime RefreshTokenExpiration { get; set; }
    public string Email { get; set; } = null!;
    public string Role { get; set; } = null!;
    public string FullName { get; set; } = null!;
}

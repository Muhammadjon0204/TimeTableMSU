namespace Application.DTOs.AuthDTOs;

public class ExternalLoginDto
{
    public string Provider { get; set; } = null!;
    public string ProviderToken { get; set; } = null!;
}

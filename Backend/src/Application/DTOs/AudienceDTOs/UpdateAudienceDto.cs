namespace Application.DTOs.AudienceDTOs;

public class UpdateAudienceDto
{
    public int Id { get; set; }
    public string Number { get; set; } = null!;
    public string Type { get; set; } = null!;
}

namespace Application.DTOs.GroupDTOs;

public class GetGroupDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public short? Course { get; set; }
    public string? SpecialityName { get; set; }
}

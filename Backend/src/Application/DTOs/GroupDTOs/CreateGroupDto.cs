namespace Application.DTOs.GroupDTOs;

public class CreateGroupDto
{
    public string Name { get; set; } = null!;
    public int SpecialityId { get; set; }
    public short? Course { get; set; }
}

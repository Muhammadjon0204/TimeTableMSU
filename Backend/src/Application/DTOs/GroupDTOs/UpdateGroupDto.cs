namespace Application.DTOs.GroupDTOs;

public class UpdateGroupDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int SpecialityId { get; set; }
    public short? Course { get; set; }
}

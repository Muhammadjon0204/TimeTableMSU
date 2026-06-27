namespace Application.DTOs.SpecialityDTOs;

public class UpdateSpecialityDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int FacultyId { get; set; }
}

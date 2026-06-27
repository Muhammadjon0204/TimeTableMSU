namespace Application.DTOs.SpecialityDTOs;

public class GetSpecialityDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int FacultyId { get; set; }
    public string FacultyName { get; set; } = null!;
}

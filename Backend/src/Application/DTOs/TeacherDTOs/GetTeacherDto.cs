using Domain.Enums;

namespace Application.DTOs.TeacherDTOs;

public class GetTeacherDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = null!;
    public Post? TeacherPost { get; set; }
}

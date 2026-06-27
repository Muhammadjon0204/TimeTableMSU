namespace Domain.Entities;

public class Group
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int SpecialityId { get; set; }
    public short Course { get; set; }
    
    public Speciality Speciality { get; set; } = null!;
    public ICollection<Student> Students { get; set; } = new List<Student>();
    public ICollection<Discipline> Disciplines { get; set; } = new List<Discipline>();
    public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
}

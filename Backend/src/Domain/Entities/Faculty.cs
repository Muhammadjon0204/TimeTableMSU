namespace Domain.Entities;

public class Faculty
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;

    public ICollection<Speciality> Specialities { get; set; } = new List<Speciality>();
}
namespace Domain.Entities;

public class Speciality
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int FacultyId { get; set; }
    
    public Faculty Faculty { get; set; } = null!; // Внешний ключ на факультет
    public ICollection<Group> Groups { get; set; } = new List<Group>(); // Ссылка на группы
}
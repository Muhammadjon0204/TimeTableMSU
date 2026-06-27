using Domain.Enums;

namespace Domain.Entities;

public class Audience
{
    public int Id { get; set; }
    public string Number { get; set; } = null!;
    public AudienceType Type { get; set; }
    
    public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
}
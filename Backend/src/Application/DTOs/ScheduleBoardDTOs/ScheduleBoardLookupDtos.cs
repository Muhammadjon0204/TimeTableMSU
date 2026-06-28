namespace Application.DTOs.ScheduleBoardDTOs;

public class LookupDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class WeekLookupDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

namespace KoalaServiceApi.Models;

public class KoalaDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int AgeYears { get; set; }
    public int AgeDays { get; set; }
    public char Gender { get; set; } = 'M';
    public int HungerLevel { get; set; }
    public string Status { get; set; } = "Healthy";
    public bool IsAlive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}

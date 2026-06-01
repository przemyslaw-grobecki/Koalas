namespace Tellemetry.Models;

public class Koala
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int AgeYears { get; set; }
    public int AgeDays { get; set; }
    public char Gender { get; set; } = 'M'; // M or F
    public int HungerLevel { get; set; } = 0; // 0-4, 0 = not hungry, 4 = starving
    public string Status { get; set; } = "Healthy"; // Healthy, Hungry, Starving, Dead
    public bool IsAlive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

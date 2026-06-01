namespace BambooServiceApi.Models;

public class BambooDto
{
    public int Id { get; set; }
    public string Species { get; set; } = string.Empty;
    public int HeightCm { get; set; }
    public int DiameterCm { get; set; }
    public string Location { get; set; } = string.Empty;
    public string HealthStatus { get; set; } = "Healthy";
    public double WeightKg { get; set; }
    public DateTime PlantedDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

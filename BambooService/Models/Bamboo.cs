namespace BambooService.Models;

public class Bamboo
{
    public int Id { get; set; }
    public string Species { get; set; } = string.Empty;
    public int HeightCm { get; set; }
    public int DiameterCm { get; set; } // Cylinder diameter
    public string Location { get; set; } = string.Empty;
    public string HealthStatus { get; set; } = "Healthy";
    public double WeightKg { get; set; } // Calculated from cylinder volume
    public DateTime PlantedDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Calculate bamboo weight based on cylinder dimensions.
    /// Formula: V = π * r² * h, Density ≈ 700 kg/m³
    /// </summary>
    public void CalculateWeight()
    {
        double radiusM = DiameterCm / 200.0; // Convert cm to m and divide by 2
        double heightM = HeightCm / 100.0; // Convert cm to m
        double volumeM3 = Math.PI * radiusM * radiusM * heightM;
        WeightKg = volumeM3 * 700; // Bamboo density approximately 700 kg/m³
    }
}

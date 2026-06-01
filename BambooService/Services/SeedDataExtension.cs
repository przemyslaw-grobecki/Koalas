using BambooService.Data;
using BambooService.Models;

namespace BambooService.Services;

public static class SeedDataExtension
{
    public static async Task SeedBambooAsync(this BambooDbContext db)
    {
        if (db.Bamboos.Any())
            return;

        var random = new Random(42);
        var bamboos = new List<Bamboo>();

        for (int i = 0; i < 500; i++)
        {
            var heightCm = random.Next(50, 151);
            var diameterCm = random.Next(3, 9);
            var density = 700.0;
            var volumeM3 = Math.PI * Math.Pow(diameterCm / 200.0, 2) * (heightCm / 100.0);
            var weightKg = volumeM3 * density;

            bamboos.Add(new Bamboo
            {
                HeightCm = heightCm,
                DiameterCm = diameterCm,
                Location = $"Section-{(i / 50) + 1}",
                HealthStatus = "Healthy",
                WeightKg = weightKg,
                PlantedDate = DateTime.UtcNow.AddDays(-random.Next(30, 180))
            });
        }

        db.Bamboos.AddRange(bamboos);
        await db.SaveChangesAsync();
    }
}

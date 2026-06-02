using BambooService.Data;
using BambooService.Models;

namespace BambooService.Services.BackgroundServices;

public class BambooPlantationBackgroundService(IServiceProvider serviceProvider, ILogger<BambooPlantationBackgroundService> logger) : BackgroundService
{

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("BambooPlantationBackgroundService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<BambooDbContext>();
                await ProduceBambooAsync(dbContext);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in BambooPlantationBackgroundService");
            }

            // Produce bamboo every minute (simulating growth)
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }

        logger.LogInformation("BambooPlantationBackgroundService stopped");
    }

    private async Task ProduceBambooAsync(BambooDbContext dbContext)
    {
        // Create new bamboo stalks
        // Realistic bamboo dimensions: height 10-15m, diameter 5-15cm
        // We'll simulate smaller harvested pieces: 50-100cm height, 3-8cm diameter

        int stalksPerMinute = 5; // Produce 5 stalks per minute

        for (int i = 0; i < stalksPerMinute; i++)
        {
            var bamboo = new Bamboo
            {
                Species = "Bambusa vulgaris",
                HeightCm = Random.Shared.Next(50, 101), // 50-100cm
                DiameterCm = Random.Shared.Next(3, 9), // 3-8cm
                Location = "Main Plantation",
                PlantedDate = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 365)),
                CreatedAt = DateTime.UtcNow
            };

            bamboo.CalculateWeight();

            dbContext.Bamboos.Add(bamboo);
        }

        await dbContext.SaveChangesAsync();
        logger.LogInformation("Produced {Count} bamboo stalks", stalksPerMinute);
    }
}
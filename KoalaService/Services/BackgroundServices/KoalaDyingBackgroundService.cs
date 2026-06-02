using Tellemetry.Data;
using Microsoft.EntityFrameworkCore;

namespace Tellemetry.Services.BackgroundServices;

public class KoalaDyingBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<KoalaDyingBackgroundService> _logger;
    private readonly Random _random = new();

    public KoalaDyingBackgroundService(IServiceProvider serviceProvider, ILogger<KoalaDyingBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("KoalaDyingBackgroundService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<KoalaDbContext>();
                    await CheckDeathAsync(dbContext);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in KoalaDyingBackgroundService");
            }

            // Check death roughly every minute (±20s) to desynchronize from other services
            var delaySeconds = 40 + Random.Shared.Next(40); // 40-79 seconds
            await Task.Delay(TimeSpan.FromSeconds(delaySeconds), stoppingToken);
        }

        _logger.LogInformation("KoalaDyingBackgroundService stopped");
    }

    private async Task CheckDeathAsync(KoalaDbContext dbContext)
    {
        var aliveKoalas = await dbContext.Koalas
            .Where(k => k.IsAlive)
            .ToListAsync();

        foreach (var koala in aliveKoalas)
        {
            // Calculate death probability based on hunger and age
            // Hunger contributes 10-50% probability (0-4 levels)
            // Age contributes additional probability (older = higher)
            // Max lifespan: ~18 years in the wild, we'll use 20 for calculation

            int hungerDeathChance = koala.HungerLevel * 10; // 0-40%
            int ageDeathChance = (int)((koala.AgeYears / 20.0) * 30); // 0-30% based on age
            int totalDeathChance = hungerDeathChance + ageDeathChance;

            if (_random.Next(100) < totalDeathChance)
            {
                koala.IsAlive = false;
                koala.Status = "Dead";

                _logger.LogWarning("Koala {KoalaName} died at age {Age}y {AgeDays}d with hunger level {HungerLevel}",
                    koala.Name, koala.AgeYears, koala.AgeDays, koala.HungerLevel);
            }
        }

        await dbContext.SaveChangesAsync();
    }
}

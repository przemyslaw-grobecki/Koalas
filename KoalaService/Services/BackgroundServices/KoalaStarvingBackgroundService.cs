using Tellemetry.Data;
using Microsoft.EntityFrameworkCore;

namespace Tellemetry.Services.BackgroundServices;

public class KoalaStarvingBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<KoalaStarvingBackgroundService> _logger;

    public KoalaStarvingBackgroundService(IServiceProvider serviceProvider, ILogger<KoalaStarvingBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("KoalaStarvingBackgroundService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<KoalaDbContext>();
                    await IncreaseHungerAsync(dbContext);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in KoalaStarvingBackgroundService");
            }

            // Increase hunger roughly every minute (±20s) to desynchronize from other services
            var delaySeconds = 40 + Random.Shared.Next(40); // 40-79 seconds
            await Task.Delay(TimeSpan.FromSeconds(delaySeconds), stoppingToken);
        }

        _logger.LogInformation("KoalaStarvingBackgroundService stopped");
    }

    private async Task IncreaseHungerAsync(KoalaDbContext dbContext)
    {
        var aliveKoalas = await dbContext.Koalas
            .Where(k => k.IsAlive)
            .ToListAsync();

        foreach (var koala in aliveKoalas)
        {
            if (koala.HungerLevel < 4)
            {
                koala.HungerLevel++;

                // Update status based on hunger level
                koala.Status = koala.HungerLevel switch
                {
                    0 => "Healthy",
                    1 or 2 => "Hungry",
                    3 or 4 => "Starving",
                    _ => "Healthy"
                };

                _logger.LogInformation("Koala {KoalaName} hunger level increased to {HungerLevel}",
                    koala.Name, koala.HungerLevel);
            }
        }

        await dbContext.SaveChangesAsync();
    }
}

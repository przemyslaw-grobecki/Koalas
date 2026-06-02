using Tellemetry.Data;
using Microsoft.EntityFrameworkCore;

namespace Tellemetry.Services.BackgroundServices;

public class KoalaAgingBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<KoalaAgingBackgroundService> _logger;

    public KoalaAgingBackgroundService(IServiceProvider serviceProvider, ILogger<KoalaAgingBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("KoalaAgingBackgroundService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<KoalaDbContext>();
                    await AgeKoalasAsync(dbContext);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in KoalaAgingBackgroundService");
            }

            // Age koalas roughly every minute (±20s) to desynchronize from other services
            var delaySeconds = 40 + Random.Shared.Next(40); // 40-79 seconds
            await Task.Delay(TimeSpan.FromSeconds(delaySeconds), stoppingToken);
        }

        _logger.LogInformation("KoalaAgingBackgroundService stopped");
    }

    private async Task AgeKoalasAsync(KoalaDbContext dbContext)
    {
        var koalas = await dbContext.Koalas
            .Where(k => k.IsAlive)
            .ToListAsync();

        foreach (var koala in koalas)
        {
            koala.AgeDays++;

            // Convert days to years
            if (koala.AgeDays >= 365)
            {
                koala.AgeYears += koala.AgeDays / 365;
                koala.AgeDays = koala.AgeDays % 365;
            }
        }

        await dbContext.SaveChangesAsync();
    }
}

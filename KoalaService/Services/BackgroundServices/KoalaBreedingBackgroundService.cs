using Tellemetry.Data;
using Tellemetry.Models;
using Microsoft.EntityFrameworkCore;

namespace Tellemetry.Services.BackgroundServices;

public class KoalaBreedingBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<KoalaBreedingBackgroundService> _logger;
    private readonly Random _random = new();

    public KoalaBreedingBackgroundService(IServiceProvider serviceProvider, ILogger<KoalaBreedingBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("KoalaBreedingBackgroundService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<KoalaDbContext>();
                    await CheckAndBreedKoalasAsync(dbContext);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in KoalaBreedingBackgroundService");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }

        _logger.LogInformation("KoalaBreedingBackgroundService stopped");
    }

    private async Task CheckAndBreedKoalasAsync(KoalaDbContext dbContext)
    {
        // Get all healthy koalas older than 2 years
        var healthyKoalas = await dbContext.Koalas
            .Where(k => k.IsAlive && k.Status == "Healthy" && k.AgeYears >= 2 && k.HungerLevel <= 2)
            .ToListAsync();

        // Separate by gender
        var females = healthyKoalas.Where(k => k.Gender == 'F').ToList();
        var males = healthyKoalas.Where(k => k.Gender == 'M').ToList();

        if (females.Count == 0 || males.Count == 0)
            return;

        // Check breeding probability (20%)
        foreach (var female in females)
        {
            if (_random.Next(100) < 20)
            {
                var male = males[_random.Next(males.Count)];
                var babyGender = _random.Next(100) < 50 ? 'F' : 'M';

                var baby = new Koala
                {
                    Name = $"Baby_{Guid.NewGuid().ToString().Substring(0, 8)}",
                    AgeYears = 0,
                    AgeDays = 0,
                    Gender = babyGender,
                    HungerLevel = 0,
                    Status = "Healthy",
                    IsAlive = true,
                    CreatedAt = DateTime.UtcNow
                };

                dbContext.Koalas.Add(baby);
                _logger.LogInformation("New koala born: {KoalaName} (Gender: {Gender}) from {MotherName} and {FatherName}",
                    baby.Name, baby.Gender, female.Name, male.Name);
            }
        }

        await dbContext.SaveChangesAsync();
    }
}

using Microsoft.EntityFrameworkCore;
using BambooService.Data;
using BambooService.Services;
using BambooService.Services.BackgroundServices;
using Shared.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(
        "logs/bamboo-service-.txt",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "BambooService")
    .CreateLogger();

try
{
    builder.Host.UseSerilog();

    // Add services to the container
    builder.Services.AddDbContext<BambooDbContext>(options =>
        options.UseInMemoryDatabase("BambooDb"));

    builder.Services.AddScoped<BambooService.Services.BambooService>();
    builder.Services.AddSingleton<IMqttService>(sp =>
        new MqttService(sp.GetRequiredService<ILogger<MqttService>>(),
                        sp.GetRequiredService<IConfiguration>(),
                        "BambooService"));

    // Add background services
    builder.Services.AddHostedService<BambooPlantationBackgroundService>();

    // Add CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll",
            builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });
    });

    builder.Services.AddLogging();

    var app = builder.Build();

    // Configure the HTTP request pipeline
    app.UseCors("AllowAll");

    // Seed initial bamboo data
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<BambooDbContext>();
        if (!db.Bamboos.Any())
        {
            Log.Information("Seeding initial bamboo plantation...");
            var random = new Random(42); // Fixed seed for reproducibility
            var bamboos = new List<BambooService.Models.Bamboo>();

            for (int i = 0; i < 500; i++)
            {
                var heightCm = random.Next(50, 151); // 50-150 cm
                var diameterCm = random.Next(3, 9); // 3-8 cm
                var density = 700; // kg/m³
                var volumeM3 = Math.PI * Math.Pow(diameterCm / 200.0, 2) * (heightCm / 100.0);
                var weightKg = volumeM3 * density;

                bamboos.Add(new BambooService.Models.Bamboo
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
            var totalWeight = bamboos.Sum(b => b.WeightKg);
            Log.Information("Seeded {BambooCount} bamboo stalks with total weight {TotalWeight}kg", bamboos.Count, Math.Round(totalWeight, 2));
        }
    }

    // Initialize MQTT connection on startup
    var mqttService = app.Services.GetRequiredService<IMqttService>();
    await mqttService.ConnectAsync();

    Log.Information("BambooService starting...");

    app.MapGet("/", () => "BambooService - Zoo Management API");
    app.MapGet("/health", () => Results.Ok("BambooService is running"));

    // Endpoints for managing bamboo
    app.MapGet("/api/bamboo", async (BambooDbContext db) =>
        await db.Bamboos.Where(b => b.HealthStatus == "Healthy").ToListAsync());

    app.MapGet("/api/bamboo/{id}", async (int id, BambooDbContext db) =>
        await db.Bamboos.FirstOrDefaultAsync(b => b.Id == id)
            is var bamboo
            ? Results.Ok(bamboo)
            : Results.NotFound());

    app.MapGet("/api/bamboo/total-weight", async (BambooDbContext db) =>
    {
        var totalWeight = await db.Bamboos
            .Where(b => b.HealthStatus == "Healthy")
            .SumAsync(b => b.WeightKg);
        var count = await db.Bamboos.Where(b => b.HealthStatus == "Healthy").CountAsync();
        return Results.Ok(new { totalWeightKg = totalWeight, stalksCount = count });
    });

    app.MapPost("/api/bamboo/consume/{weight}", async (double weight, BambooDbContext db) =>
    {
        var totalWeight = await db.Bamboos
            .Where(b => b.HealthStatus == "Healthy")
            .SumAsync(b => b.WeightKg);

        if (totalWeight < weight)
            return Results.BadRequest($"Not enough bamboo. Available: {totalWeight}kg, Requested: {weight}kg");

        // Consume bamboo starting from oldest
        var bambooToConsume = await db.Bamboos
            .Where(b => b.HealthStatus == "Healthy")
            .OrderBy(b => b.PlantedDate)
            .ToListAsync();

        double remainingWeight = weight;
        var consumed = new List<int>();

        foreach (var bamboo in bambooToConsume)
        {
            if (remainingWeight <= 0)
                break;

            if (bamboo.WeightKg <= remainingWeight)
            {
                remainingWeight -= bamboo.WeightKg;
                db.Bamboos.Remove(bamboo);
                consumed.Add(bamboo.Id);
            }
            else
            {
                // Partial consumption
                bamboo.WeightKg -= remainingWeight;
                remainingWeight = 0;
            }
        }

        await db.SaveChangesAsync();
        return Results.Ok(new { consumedKg = weight, consumedBambooIds = consumed });
    });

    app.MapGet("/api/bamboo/stats", async (BambooDbContext db) =>
    {
        var stats = new
        {
            totalStalkCount = await db.Bamboos.CountAsync(),
            healthyStalkCount = await db.Bamboos.CountAsync(b => b.HealthStatus == "Healthy"),
            totalWeightKg = await db.Bamboos.Where(b => b.HealthStatus == "Healthy").SumAsync(b => b.WeightKg),
            averageWeightPerStalk = await db.Bamboos.Where(b => b.HealthStatus == "Healthy").AverageAsync(b => b.WeightKg),
            averageHeightCm = await db.Bamboos.Where(b => b.HealthStatus == "Healthy").AverageAsync(b => b.HeightCm),
            averageDiameterCm = await db.Bamboos.Where(b => b.HealthStatus == "Healthy").AverageAsync(b => b.DiameterCm)
        };
        return Results.Ok(stats);
    });

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "BambooService terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}

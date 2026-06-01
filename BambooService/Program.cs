using Microsoft.EntityFrameworkCore;
using BambooService.Data;
using BambooService.Services;
using BambooService.Services.BackgroundServices;
using Shared.Services;

var builder = WebApplication.CreateBuilder(args);

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

// Initialize MQTT connection on startup
var mqttService = app.Services.GetRequiredService<IMqttService>();
await mqttService.ConnectAsync();

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

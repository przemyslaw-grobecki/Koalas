using Microsoft.EntityFrameworkCore;
using Tellemetry.Data;
using Tellemetry.Services;
using Tellemetry.Services.BackgroundServices;
using Shared.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddDbContext<KoalaDbContext>(options =>
    options.UseInMemoryDatabase("KoalaDb"));

builder.Services.AddScoped<KoalaService>();
builder.Services.AddSingleton<IMqttService>(sp =>
    new MqttService(sp.GetRequiredService<ILogger<MqttService>>(), 
                    sp.GetRequiredService<IConfiguration>(),
                    "KoalaService"));

// Add background services
builder.Services.AddHostedService<KoalaBreedingBackgroundService>();
builder.Services.AddHostedService<KoalaAgingBackgroundService>();
builder.Services.AddHostedService<KoalaStarvingBackgroundService>();
builder.Services.AddHostedService<KoalaDyingBackgroundService>();

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

app.MapGet("/", () => "KoalaService - Zoo Management API");
app.MapGet("/health", () => Results.Ok("KoalaService is running"));

// Endpoints for managing koalas
app.MapGet("/api/koalas", async (KoalaDbContext db) =>
    await db.Koalas.Where(k => k.IsAlive).ToListAsync());

app.MapGet("/api/koalas/{id}", async (int id, KoalaDbContext db) =>
    await db.Koalas.FirstOrDefaultAsync(k => k.Id == id)
        is var koala
        ? Results.Ok(koala)
        : Results.NotFound());

app.MapPost("/api/koalas/feed/{id}", async (int id, KoalaDbContext db) =>
{
    var koala = await db.Koalas.FirstOrDefaultAsync(k => k.Id == id && k.IsAlive);
    if (koala == null)
        return Results.NotFound("Koala not found or is dead");

    if (koala.HungerLevel > 0)
    {
        koala.HungerLevel--;
        koala.Status = koala.HungerLevel switch
        {
            0 => "Healthy",
            1 or 2 => "Hungry",
            3 or 4 => "Starving",
            _ => "Healthy"
        };
        await db.SaveChangesAsync();
        return Results.Ok(new { message = $"Koala {koala.Name} fed successfully", koala });
    }

    return Results.BadRequest("Koala is not hungry");
});

app.MapGet("/api/koalas/stats", async (KoalaDbContext db) =>
{
    var stats = new
    {
        totalAlive = await db.Koalas.CountAsync(k => k.IsAlive),
        totalDead = await db.Koalas.CountAsync(k => !k.IsAlive),
        totalAll = await db.Koalas.CountAsync(),
        byStatus = await db.Koalas
            .Where(k => k.IsAlive)
            .GroupBy(k => k.Status)
            .Select(g => new { status = g.Key, count = g.Count() })
            .ToListAsync(),
        averageAge = await db.Koalas
            .Where(k => k.IsAlive)
            .AverageAsync(k => k.AgeYears),
        averageHunger = await db.Koalas
            .Where(k => k.IsAlive)
            .AverageAsync(k => k.HungerLevel)
    };
    return Results.Ok(stats);
});

app.Run();

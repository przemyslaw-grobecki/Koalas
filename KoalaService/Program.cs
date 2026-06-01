using Microsoft.EntityFrameworkCore;
using Tellemetry.Data;
using Tellemetry.Services;
using Tellemetry.Services.BackgroundServices;
using Shared.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(
        "logs/koala-service-.txt",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "KoalaService")
    .CreateLogger();

try
{
    builder.Host.UseSerilog();

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

    // Seed initial koala data
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<KoalaDbContext>();
        if (!db.Koalas.Any())
        {
            Log.Information("Seeding initial koala population...");
            var koalas = new List<Tellemetry.Models.Koala>
            {
                new() { Name = "Koa", AgeYears = 5, AgeDays = 0, Gender = 'M', HungerLevel = 1, Status = "Healthy", IsAlive = true },
                new() { Name = "Bea", AgeYears = 4, AgeDays = 0, Gender = 'F', HungerLevel = 0, Status = "Healthy", IsAlive = true },
                new() { Name = "Leo", AgeYears = 3, AgeDays = 0, Gender = 'M', HungerLevel = 2, Status = "Hungry", IsAlive = true },
                new() { Name = "Luna", AgeYears = 2, AgeDays = 0, Gender = 'F', HungerLevel = 1, Status = "Healthy", IsAlive = true },
                new() { Name = "Milo", AgeYears = 6, AgeDays = 0, Gender = 'M', HungerLevel = 0, Status = "Healthy", IsAlive = true },
                new() { Name = "Gigi", AgeYears = 1, AgeDays = 0, Gender = 'F', HungerLevel = 3, Status = "Starving", IsAlive = true },
                new() { Name = "Max", AgeYears = 8, AgeDays = 0, Gender = 'M', HungerLevel = 1, Status = "Healthy", IsAlive = true },
                new() { Name = "Nala", AgeYears = 3, AgeDays = 0, Gender = 'F', HungerLevel = 2, Status = "Hungry", IsAlive = true },
                new() { Name = "Cody", AgeYears = 4, AgeDays = 0, Gender = 'M', HungerLevel = 1, Status = "Healthy", IsAlive = true },
                new() { Name = "Ellie", AgeYears = 2, AgeDays = 0, Gender = 'F', HungerLevel = 0, Status = "Healthy", IsAlive = true },
                new() { Name = "Bash", AgeYears = 5, AgeDays = 0, Gender = 'M', HungerLevel = 2, Status = "Hungry", IsAlive = true },
                new() { Name = "Penny", AgeYears = 3, AgeDays = 0, Gender = 'F', HungerLevel = 1, Status = "Healthy", IsAlive = true },
                new() { Name = "Oscar", AgeYears = 1, AgeDays = 0, Gender = 'M', HungerLevel = 3, Status = "Starving", IsAlive = true },
                new() { Name = "Zoe", AgeYears = 6, AgeDays = 0, Gender = 'F', HungerLevel = 0, Status = "Healthy", IsAlive = true },
                new() { Name = "Tucker", AgeYears = 4, AgeDays = 0, Gender = 'M', HungerLevel = 1, Status = "Healthy", IsAlive = true }
            };
            db.Koalas.AddRange(koalas);
            await db.SaveChangesAsync();
            Log.Information("Seeded {KoalaCount} koalas", koalas.Count);
        }
    }

    // Initialize MQTT connection on startup
    var mqttService = app.Services.GetRequiredService<IMqttService>();
    await mqttService.ConnectAsync();

    Log.Information("KoalaService starting...");

    app.MapGet("/", () => "KoalaService - Zoo Management API");
    app.MapGet("/health", () => Results.Ok("KoalaService is running"));

    // Endpoints for managing koalas
    app.MapGet("/api/koalas", async (KoalaDbContext db) =>
        await db.Koalas.Where(k => k.IsAlive).ToListAsync());

    app.MapGet("/api/koalas/{id}", async (int id, KoalaDbContext db) =>
        await db.Koalas.FirstOrDefaultAsync(k => k.Id == id && k.IsAlive)
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
}
catch (Exception ex)
{
    Log.Fatal(ex, "KoalaService terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}

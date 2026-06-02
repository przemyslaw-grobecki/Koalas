using Microsoft.EntityFrameworkCore;
using Tellemetry.Data;
using Tellemetry.Services;
using Tellemetry.Services.BackgroundServices;
using Shared.Models;
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
    builder.Services.Configure<PostgresOptions>(
        builder.Configuration.GetSection(PostgresOptions.SectionName));

    builder.Services.AddDbContext<KoalaDbContext>();

    builder.Services.AddScoped<KoalaService>();
    builder.Services.AddScoped<IKoalaController, KoalaController>();
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

    // Seed data on startup
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<KoalaDbContext>();
        await db.Database.EnsureCreatedAsync();
        Log.Information("Seeding initial koala population...");
        await db.SeedKoalasAsync();
        Log.Information("Koala population seeded");
    }

    // Initialize MQTT connection on startup (non-fatal if broker unavailable)
    try
    {
        var mqttService = app.Services.GetRequiredService<IMqttService>();
        await mqttService.ConnectAsync();
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Could not connect to MQTT broker. Continuing without MQTT.");
    }

    Log.Information("KoalaService starting...");

    app.MapGet("/", () => "KoalaService - Zoo Management API");
    app.MapGet("/health", () => Results.Ok("KoalaService is running"));

    // Endpoints for managing koalas - all using IKoalaController
    app.MapGet("/api/koalas", async (IKoalaController controller) =>
    {
        var koalas = await controller.GetAllAliveKoalasAsync();
        return Results.Ok(koalas);
    });

    app.MapGet("/api/koalas/{id}", async (int id, IKoalaController controller) =>
    {
        var koala = await controller.GetAliveKoalaByIdAsync(id);
        return koala is not null ? Results.Ok(koala) : Results.NotFound();
    });

    app.MapPost("/api/koalas/feed/{id}", async (int id, IKoalaController controller) =>
    {
        try
        {
            var result = await controller.FeedKoalaAsync(id);
            return Results.Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return Results.NotFound("Koala not found or is dead");
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(ex.Message);
        }
    });

    app.MapGet("/api/koalas/stats", async (IKoalaController controller) =>
    {
        var stats = await controller.GetStatsAsync();
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
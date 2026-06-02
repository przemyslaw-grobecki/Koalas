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
    builder.Services.AddScoped<IBambooController, BambooController>();
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

    // Seed data on startup
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<BambooDbContext>();
        Log.Information("Seeding initial bamboo plantation...");
        await db.SeedBambooAsync();
        Log.Information("Bamboo plantation seeded");
    }

    // Initialize MQTT connection on startup
    var mqttService = app.Services.GetRequiredService<IMqttService>();
    await mqttService.ConnectAsync();

    Log.Information("BambooService starting...");

    app.MapGet("/", () => "BambooService - Zoo Management API");
    app.MapGet("/health", () => Results.Ok("BambooService is running"));

    // Endpoints for managing bamboo - all using IBambooController
    app.MapGet("/api/bamboo", async (IBambooController controller) =>
    {
        var bamboo = await controller.GetAllHealthyBambooAsync();
        return Results.Ok(bamboo);
    });

    app.MapGet("/api/bamboo/all", async (IBambooController controller) =>
    {
        var bamboo = await controller.GetAllBambooAsync();
        return Results.Ok(bamboo);
    });

    app.MapGet("/api/bamboo/{id}", async (int id, IBambooController controller) =>
    {
        var bamboo = await controller.GetBambooByIdAsync(id);
        return bamboo is not null ? Results.Ok(bamboo) : Results.NotFound();
    });

    app.MapGet("/api/bamboo/total-weight", async (IBambooController controller) =>
    {
        var result = await controller.GetTotalWeightAsync();
        return Results.Ok(result);
    });

    app.MapPost("/api/bamboo/consume/{weight}", async (double weight, IBambooController controller) =>
    {
        try
        {
            var result = await controller.ConsumeBambooAsync(weight);
            return Results.Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(ex.Message);
        }
    });

    app.MapGet("/api/bamboo/stats", async (IBambooController controller) =>
    {
        var stats = await controller.GetStatsAsync();
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
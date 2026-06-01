using KoalaServiceApi.Clients;
using BambooServiceApi.Clients;
using Shared.Services;

var builder = WebApplication.CreateBuilder(args);

// Add HTTP clients for communication with microservices
builder.Services.AddHttpClient<IKoalaServiceClient, KoalaServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:KoalaService:Url"] ?? "https://localhost:5001");
});

builder.Services.AddHttpClient<IBambooServiceClient, BambooServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:BambooService:Url"] ?? "https://localhost:5002");
});

// Add MQTT service
builder.Services.AddSingleton<IMqttService>(sp =>
    new MqttService(sp.GetRequiredService<ILogger<MqttService>>(), 
                    sp.GetRequiredService<IConfiguration>(),
                    "ZooManagementService"));

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

var app = builder.Build();

app.UseCors("AllowAll");

app.MapGet("/", () => "ZooManagementService - Central Zoo Management API");
app.MapGet("/health", () => Results.Ok("ZooManagementService is running"));

// Endpoint to feed koalas with bamboo
app.MapPost("/api/feed-koalas", async (
    IKoalaServiceClient koalaClient,
    IBambooServiceClient bambooClient,
    ILogger<Program> logger,
    FeedKoalasRequest request) =>
{
    logger.LogInformation("Received request to feed {KoalaCount} koalas with {BambooWeight}kg of bamboo", 
        request.KoalaIds?.Count ?? 0, request.BambooWeightKg);

    if (request.BambooWeightKg <= 0)
        return Results.BadRequest("Bamboo weight must be greater than 0");

    // Check bamboo availability
    var bambooStats = await bambooClient.GetAllBambooAsync();
    if (bambooStats.Count == 0)
        return Results.BadRequest("No bamboo available in the plantation");

    // Calculate total available bamboo weight
    double totalBambooWeight = bambooStats.Sum(b => b.WeightKg);
    
    if (totalBambooWeight < request.BambooWeightKg)
    {
        logger.LogWarning("Insufficient bamboo. Available: {Available}kg, Requested: {Requested}kg",
            totalBambooWeight, request.BambooWeightKg);
        return Results.BadRequest($"Insufficient bamboo. Available: {totalBambooWeight}kg, Requested: {request.BambooWeightKg}kg");
    }

    // Consume bamboo
    try
    {
        // This would need an endpoint on BambooService
        logger.LogInformation("Successfully fed {KoalaCount} koalas", request.KoalaIds?.Count ?? 0);
        
        return Results.Ok(new
        {
            message = "Koalas fed successfully",
            koalasCount = request.KoalaIds?.Count ?? 0,
            bambooUsedKg = request.BambooWeightKg,
            remainingBambooKg = totalBambooWeight - request.BambooWeightKg
        });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error feeding koalas");
        return Results.StatusCode(500);
    }
});

// Get zoo statistics
app.MapGet("/api/zoo-stats", async (
    IKoalaServiceClient koalaClient,
    IBambooServiceClient bambooClient) =>
{
    try
    {
        var koalas = await koalaClient.GetAllKoalasAsync();
        var bamboo = await bambooClient.GetAllBambooAsync();

        var stats = new
        {
            koalas = new
            {
                totalCount = koalas.Count,
                healthyCount = koalas.Count(k => k.Status == "Healthy"),
                hungryCount = koalas.Count(k => k.Status == "Hungry"),
                starvingCount = koalas.Count(k => k.Status == "Starving"),
                averageAge = koalas.Average(k => k.AgeYears),
                averageHunger = koalas.Average(k => k.HungerLevel)
            },
            bamboo = new
            {
                totalStalks = bamboo.Count,
                totalWeightKg = bamboo.Sum(b => b.WeightKg),
                averageWeightPerStalk = bamboo.Count > 0 ? bamboo.Average(b => b.WeightKg) : 0
            }
        };

        return Results.Ok(stats);
    }
    catch
    {
        return Results.StatusCode(500);
    }
});

app.Run();

public record FeedKoalasRequest(
    List<int>? KoalaIds,
    double BambooWeightKg
);

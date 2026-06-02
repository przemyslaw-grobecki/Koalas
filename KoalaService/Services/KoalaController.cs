using Tellemetry.Models;

namespace Tellemetry.Services;

public class KoalaController(KoalaService koalaService, ILogger<KoalaController> logger) : IKoalaController
{
    public async Task<List<Koala>> GetAllAliveKoalasAsync()
    {
        logger.LogInformation("Getting all alive koalas via controller");
        return await koalaService.GetAllAliveKoalasAsync();
    }

    public async Task<Koala?> GetAliveKoalaByIdAsync(int id)
    {
        logger.LogInformation("Getting alive koala with id: {KoalaId} via controller", id);
        return await koalaService.GetAliveKoalaByIdAsync(id);
    }

    public async Task<object> FeedKoalaAsync(int id)
    {
        logger.LogInformation("Feeding koala with id: {KoalaId} via controller", id);
        return await koalaService.FeedKoalaAsync(id);
    }

    public async Task<object> GetStatsAsync()
    {
        logger.LogInformation("Getting koala stats via controller");
        return await koalaService.GetStatsAsync();
    }
}
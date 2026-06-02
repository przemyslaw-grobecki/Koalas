using BambooService.Models;

namespace BambooService.Services;

public class BambooController(BambooService bambooService, ILogger<BambooController> logger) : IBambooController
{
    public async Task<List<Bamboo>> GetAllBambooAsync()
    {
        logger.LogInformation("Getting all bamboo via controller");
        return await bambooService.GetAllBambooAsync();
    }

    public async Task<Bamboo?> GetBambooByIdAsync(int id)
    {
        logger.LogInformation("Getting bamboo with id: {BambooId} via controller", id);
        return await bambooService.GetBambooByIdAsync(id);
    }

    public async Task<object> GetTotalWeightAsync()
    {
        logger.LogInformation("Getting total bamboo weight via controller");
        return await bambooService.GetTotalWeightAsync();
    }

    public async Task<object> ConsumeBambooAsync(double weight)
    {
        logger.LogInformation("Consuming {Weight}kg of bamboo via controller", weight);
        return await bambooService.ConsumeBambooAsync(weight);
    }

    public async Task<object> GetStatsAsync()
    {
        logger.LogInformation("Getting bamboo stats via controller");
        return await bambooService.GetStatsAsync();
    }
}
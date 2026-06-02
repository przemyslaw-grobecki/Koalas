using BambooService.Data;
using BambooService.Models;
using Microsoft.EntityFrameworkCore;

namespace BambooService.Services;

public class BambooService(BambooDbContext dbContext, ILogger<BambooService> logger)
{
    public async Task<List<Bamboo>> GetAllBambooAsync()
    {
        logger.LogInformation("Getting all bamboo plants");
        return await dbContext.Bamboos.ToListAsync();
    }

    public async Task<Bamboo?> GetBambooByIdAsync(int id)
    {
        logger.LogInformation("Getting bamboo with id: {BambooId}", id);
        return await dbContext.Bamboos.FindAsync(id);
    }

    public async Task<Bamboo> CreateBambooAsync(Bamboo bamboo)
    {
        logger.LogInformation("Creating new bamboo: {BambooSpecies}", bamboo.Species);
        dbContext.Bamboos.Add(bamboo);
        await dbContext.SaveChangesAsync();
        return bamboo;
    }

    public async Task<Bamboo?> UpdateBambooAsync(int id, Bamboo bamboo)
    {
        logger.LogInformation("Updating bamboo with id: {BambooId}", id);
        var existingBamboo = await dbContext.Bamboos.FindAsync(id);
        if (existingBamboo == null)
            return null;

        existingBamboo.Species = bamboo.Species;
        existingBamboo.HeightCm = bamboo.HeightCm;
        existingBamboo.Location = bamboo.Location;
        existingBamboo.PlantedDate = bamboo.PlantedDate;

        await dbContext.SaveChangesAsync();
        return existingBamboo;
    }

    public async Task<bool> DeleteBambooAsync(int id)
    {
        logger.LogInformation("Deleting bamboo with id: {BambooId}", id);
        var bamboo = await dbContext.Bamboos.FindAsync(id);
        if (bamboo == null)
            return false;

        dbContext.Bamboos.Remove(bamboo);
        await dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<object> GetTotalWeightAsync()
    {
        logger.LogInformation("Getting total bamboo weight");
        var totalWeight = await dbContext.Bamboos.SumAsync(b => b.WeightKg);
        var count = await dbContext.Bamboos.CountAsync();
        return new { totalWeightKg = totalWeight, stalksCount = count };
    }

    public async Task<object> ConsumeBambooAsync(double weight)
    {
        logger.LogInformation("Consuming {Weight}kg of bamboo", weight);
        var totalWeight = await dbContext.Bamboos.SumAsync(b => b.WeightKg);

        if (totalWeight < weight)
            throw new InvalidOperationException($"Not enough bamboo. Available: {totalWeight}kg, Requested: {weight}kg");

        var bambooToConsume = await dbContext.Bamboos
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
                dbContext.Bamboos.Remove(bamboo);
                consumed.Add(bamboo.Id);
            }
            else
            {
                bamboo.WeightKg -= remainingWeight;
                remainingWeight = 0;
            }
        }

        await dbContext.SaveChangesAsync();
        return new { consumedKg = weight, consumedBambooIds = consumed };
    }

    public async Task<object> GetStatsAsync()
    {
        logger.LogInformation("Getting bamboo stats");
        var stats = new
        {
            totalStalkCount = await dbContext.Bamboos.CountAsync(),
            totalWeightKg = await dbContext.Bamboos.SumAsync(b => b.WeightKg),
            averageWeightPerStalk = await dbContext.Bamboos.AverageAsync(b => b.WeightKg),
            averageHeightCm = await dbContext.Bamboos.AverageAsync(b => b.HeightCm),
            averageDiameterCm = await dbContext.Bamboos.AverageAsync(b => b.DiameterCm)
        };
        return stats;
    }
}
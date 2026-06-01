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
        existingBamboo.HealthStatus = bamboo.HealthStatus;
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
}

using BambooService.Data;
using BambooService.Models;
using Microsoft.EntityFrameworkCore;

namespace BambooService.Services;

public class BambooService
{
    private readonly BambooDbContext _dbContext;
    private readonly ILogger<BambooService> _logger;

    public BambooService(BambooDbContext dbContext, ILogger<BambooService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<List<Bamboo>> GetAllBambooAsync()
    {
        _logger.LogInformation("Getting all bamboo plants");
        return await _dbContext.Bamboos.ToListAsync();
    }

    public async Task<Bamboo?> GetBambooByIdAsync(int id)
    {
        _logger.LogInformation("Getting bamboo with id: {BambooId}", id);
        return await _dbContext.Bamboos.FindAsync(id);
    }

    public async Task<Bamboo> CreateBambooAsync(Bamboo bamboo)
    {
        _logger.LogInformation("Creating new bamboo: {BambooSpecies}", bamboo.Species);
        _dbContext.Bamboos.Add(bamboo);
        await _dbContext.SaveChangesAsync();
        return bamboo;
    }

    public async Task<Bamboo?> UpdateBambooAsync(int id, Bamboo bamboo)
    {
        _logger.LogInformation("Updating bamboo with id: {BambooId}", id);
        var existingBamboo = await _dbContext.Bamboos.FindAsync(id);
        if (existingBamboo == null)
            return null;

        existingBamboo.Species = bamboo.Species;
        existingBamboo.HeightCm = bamboo.HeightCm;
        existingBamboo.Location = bamboo.Location;
        existingBamboo.HealthStatus = bamboo.HealthStatus;
        existingBamboo.PlantedDate = bamboo.PlantedDate;

        await _dbContext.SaveChangesAsync();
        return existingBamboo;
    }

    public async Task<bool> DeleteBambooAsync(int id)
    {
        _logger.LogInformation("Deleting bamboo with id: {BambooId}", id);
        var bamboo = await _dbContext.Bamboos.FindAsync(id);
        if (bamboo == null)
            return false;

        _dbContext.Bamboos.Remove(bamboo);
        await _dbContext.SaveChangesAsync();
        return true;
    }
}

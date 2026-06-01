using Tellemetry.Data;
using Tellemetry.Models;
using Microsoft.EntityFrameworkCore;

namespace Tellemetry.Services;

public class KoalaService
{
    private readonly KoalaDbContext _dbContext;
    private readonly ILogger<KoalaService> _logger;

    public KoalaService(KoalaDbContext dbContext, ILogger<KoalaService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<List<Koala>> GetAllKoalasAsync()
    {
        _logger.LogInformation("Getting all koalas");
        return await _dbContext.Koalas.ToListAsync();
    }

    public async Task<Koala?> GetKoalaByIdAsync(int id)
    {
        _logger.LogInformation("Getting koala with id: {KoalaId}", id);
        return await _dbContext.Koalas.FindAsync(id);
    }

    public async Task<Koala> CreateKoalaAsync(Koala koala)
    {
        _logger.LogInformation("Creating new koala: {KoalaName}", koala.Name);
        _dbContext.Koalas.Add(koala);
        await _dbContext.SaveChangesAsync();
        return koala;
    }

    public async Task<Koala?> UpdateKoalaAsync(int id, Koala koala)
    {
        _logger.LogInformation("Updating koala with id: {KoalaId}", id);
        var existingKoala = await _dbContext.Koalas.FindAsync(id);
        if (existingKoala == null)
            return null;

        existingKoala.Name = koala.Name;
        existingKoala.AgeYears = koala.AgeYears;
        existingKoala.AgeDays = koala.AgeDays;
        existingKoala.Gender = koala.Gender;
        existingKoala.HungerLevel = koala.HungerLevel;
        existingKoala.Status = koala.Status;
        existingKoala.IsAlive = koala.IsAlive;

        await _dbContext.SaveChangesAsync();
        return existingKoala;
    }

    public async Task<bool> DeleteKoalaAsync(int id)
    {
        _logger.LogInformation("Deleting koala with id: {KoalaId}", id);
        var koala = await _dbContext.Koalas.FindAsync(id);
        if (koala == null)
            return false;

        _dbContext.Koalas.Remove(koala);
        await _dbContext.SaveChangesAsync();
        return true;
    }
}

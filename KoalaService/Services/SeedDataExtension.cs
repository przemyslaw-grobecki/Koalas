using Tellemetry.Data;
using Tellemetry.Models;

namespace Tellemetry.Services;

public static class SeedDataExtension
{
    public static async Task SeedKoalasAsync(this KoalaDbContext db)
    {
        if (db.Koalas.Any())
            return;

        var koalas = new List<Koala>
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
    }
}

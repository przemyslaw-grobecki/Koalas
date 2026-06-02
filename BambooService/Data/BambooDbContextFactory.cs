using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BambooService.Data;

public class BambooDbContextFactory : IDesignTimeDbContextFactory<BambooDbContext>
{
    public BambooDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<BambooDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=BambooDb;Username=postgres;Password=admin");

        return new BambooDbContext(optionsBuilder.Options);
    }
}

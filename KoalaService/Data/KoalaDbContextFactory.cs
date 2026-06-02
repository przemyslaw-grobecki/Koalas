using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Tellemetry.Data;

public class KoalaDbContextFactory : IDesignTimeDbContextFactory<KoalaDbContext>
{
    public KoalaDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<KoalaDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=KoalaDb;Username=postgres;Password=admin");

        return new KoalaDbContext(optionsBuilder.Options);
    }
}

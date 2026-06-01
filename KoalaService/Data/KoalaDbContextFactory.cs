using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Tellemetry.Data;

public class KoalaDbContextFactory : IDesignTimeDbContextFactory<KoalaDbContext>
{
    public KoalaDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<KoalaDbContext>();
        optionsBuilder.UseInMemoryDatabase("KoalaDb");

        return new KoalaDbContext(optionsBuilder.Options);
    }
}

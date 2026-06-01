using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BambooService.Data;

public class BambooDbContextFactory : IDesignTimeDbContextFactory<BambooDbContext>
{
    public BambooDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<BambooDbContext>();
        optionsBuilder.UseInMemoryDatabase("BambooDb");

        return new BambooDbContext(optionsBuilder.Options);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Shared.Models;

namespace Tellemetry.Data;

public class KoalaDbContextFactory : IDesignTimeDbContextFactory<KoalaDbContext>
{
    public KoalaDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var postgresOptions = new PostgresOptions();
        configuration.GetSection(PostgresOptions.SectionName).Bind(postgresOptions);

        var optionsBuilder = new DbContextOptionsBuilder<KoalaDbContext>();
        return new KoalaDbContext(
            optionsBuilder.Options,
            Options.Create(postgresOptions));
    }
}
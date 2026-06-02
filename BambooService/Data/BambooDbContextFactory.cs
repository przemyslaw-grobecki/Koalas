using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Shared.Models;

namespace BambooService.Data;

public class BambooDbContextFactory : IDesignTimeDbContextFactory<BambooDbContext>
{
    public BambooDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var postgresOptions = new PostgresOptions();
        configuration.GetSection(PostgresOptions.SectionName).Bind(postgresOptions);

        var optionsBuilder = new DbContextOptionsBuilder<BambooDbContext>();
        return new BambooDbContext(
            optionsBuilder.Options,
            Options.Create(postgresOptions));
    }
}
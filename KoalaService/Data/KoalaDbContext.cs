using Microsoft.EntityFrameworkCore;
using Tellemetry.Models;
using Microsoft.Extensions.Options;
using Shared.Models;

namespace Tellemetry.Data;

public class KoalaDbContext : DbContext
{
    private readonly PostgresOptions _postgresOptions;

    public DbSet<Koala> Koalas { get; set; } = null!;

    public KoalaDbContext(DbContextOptions<KoalaDbContext> options, IOptions<PostgresOptions> postgresOptions)
        : base(options)
    {
        _postgresOptions = postgresOptions.Value;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql(_postgresOptions.ConnectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Koala>()
            .HasKey(k => k.Id);

        modelBuilder.Entity<Koala>()
            .Property(k => k.Name)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder.Entity<Koala>()
            .Property(k => k.Status)
            .IsRequired()
            .HasMaxLength(50);
    }
}
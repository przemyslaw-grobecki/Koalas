using Microsoft.EntityFrameworkCore;
using BambooService.Models;
using Microsoft.Extensions.Options;
using Shared.Models;

namespace BambooService.Data;

public class BambooDbContext : DbContext
{
    private readonly PostgresOptions _postgresOptions;

    public DbSet<Bamboo> Bamboos { get; set; } = null!;

    public BambooDbContext(DbContextOptions<BambooDbContext> options, IOptions<PostgresOptions> postgresOptions)
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

        modelBuilder.Entity<Bamboo>()
            .HasKey(b => b.Id);

        modelBuilder.Entity<Bamboo>()
            .Property(b => b.Species)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder.Entity<Bamboo>()
            .Property(b => b.Location)
            .IsRequired()
            .HasMaxLength(100);
    }
}
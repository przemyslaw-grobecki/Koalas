using Microsoft.EntityFrameworkCore;
using BambooService.Models;

namespace BambooService.Data;

public class BambooDbContext : DbContext
{
    public DbSet<Bamboo> Bamboos { get; set; } = null!;

    public BambooDbContext(DbContextOptions<BambooDbContext> options)
        : base(options)
    {
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

        modelBuilder.Entity<Bamboo>()
            .Property(b => b.HealthStatus)
            .IsRequired()
            .HasMaxLength(50);
    }
}

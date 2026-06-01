using Microsoft.EntityFrameworkCore;
using Tellemetry.Models;

namespace Tellemetry.Data;

public class KoalaDbContext : DbContext
{
    public DbSet<Koala> Koalas { get; set; } = null!;

    public KoalaDbContext(DbContextOptions<KoalaDbContext> options)
        : base(options)
    {
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

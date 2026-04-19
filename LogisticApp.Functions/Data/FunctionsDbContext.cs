using Microsoft.EntityFrameworkCore;
using LogisticApp.Functions.Models;

namespace LogisticApp.Functions.Data;

public class FunctionsDbContext(DbContextOptions<FunctionsDbContext> options) : DbContext(options)
{
    public DbSet<Delivery> Deliveries => Set<Delivery>();
    public DbSet<Client>   Clients    => Set<Client>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Client>(entity =>
        {
            entity.ToTable("Clients");
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<Delivery>(entity =>
        {
            entity.ToTable("Deliveries");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).HasConversion<string>();
            entity.HasOne(e => e.Client)
                  .WithMany()
                  .HasForeignKey(e => e.ClientId);
            entity.OwnsMany(e => e.ContainerNumbers, c =>
            {
                c.ToTable("ContainerEntry");
                c.Property(x => x.Number).IsRequired().HasMaxLength(50);
            });
        });
    }
}

using Microsoft.EntityFrameworkCore;

namespace PetShelterBackend.Models;

public class PetShelterContext : DbContext
{
    public DbSet<Shelter> Shelters => Set<Shelter>();
    public DbSet<Pet> Pets => Set<Pet>();

    public PetShelterContext(DbContextOptions<PetShelterContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Shelter>()
            .HasKey(r => r.Id);
        modelBuilder.Entity<Pet>()
            .HasKey(r => r.Id);
    }
}
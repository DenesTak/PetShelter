using Microsoft.EntityFrameworkCore;
using PetShelterBackend.Models.PostgreSqlModel;

namespace PetShelterBackend.Models;

public class PetShelterContext : DbContext
{
    public DbSet<PetPost> Pets { get; set; }
    public DbSet<PetShelterPost> Shelters { get; set; }
    
    public PetShelterContext(DbContextOptions<PetShelterContext> options)  : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PetPost>()
            .HasOne(p => p.Shelter)
            .WithMany(s => s.PetsInShelter)
            .HasForeignKey(p => p.ShelterId)
            .IsRequired(false); // Allow null values for ShelterId
        
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<PetShelterPost>()
            .HasKey(r => r.Id);
        modelBuilder.Entity<PetPost>()
            .HasKey(r => r.Id);
    }
}
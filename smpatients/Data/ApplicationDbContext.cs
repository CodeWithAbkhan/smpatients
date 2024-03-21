using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace smpatients.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Patient> patients { get; set; }
    public DbSet<Reviews> reviews { get; set; }
    public DbSet<PatientFiles> patientFiles { get; set; }
    public DbSet<PatientImage> patientImages { get; set; }
    public DbSet<AccessRole> accessRoles { get; set; }
   protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

          modelBuilder.Entity<PatientFiles>()
            .HasOne(pf => pf.Patient)
            .WithMany(p => p.Files)
            .HasForeignKey(pf => pf.PatientId)
            .IsRequired(false) // Optional if AppId can be nullable
            .OnDelete(DeleteBehavior.Cascade); // Configure cascade delete behavior if needed

       modelBuilder.Entity<Reviews>()
        .HasMany(c => c.Files)                // Define the navigation property to the child entity (PatientFiles)
        .WithOne(f => f.Reviews)       // Define the navigation property to the parent entity (Reviews) in the child entity (PatientFiles)
        .HasForeignKey(f => f.ReviewsId)  // Specify the foreign key property in the child entity (PatientFiles)
        .IsRequired(false)
        .OnDelete(DeleteBehavior.Cascade);    // Configure cascade delete behavior
    }
    

}


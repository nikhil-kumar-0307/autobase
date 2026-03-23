using autobase.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace autobase.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Make EmployeeNumber unique in the database
            modelBuilder.Entity<User>()
                .HasIndex(u => u.EmployeeNumber)
                .IsUnique();

            // Make MobileNumber unique
            modelBuilder.Entity<User>()
                .HasIndex(u => u.MobileNumber)
                .IsUnique();
        }


        }
    }

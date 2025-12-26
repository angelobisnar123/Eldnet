using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApplication1_Test1.Models;

namespace WebApplication1_Test1.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Database Tables
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<LockerReservationModel> LockerReservations { get; set; }
        public DbSet<ActivityReservationModel> ActivityReservations { get; set; }
        public DbSet<GatePass> GatePasses { get; set; }
        public DbSet<UserInfoModel> UserInfos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Important: Call base first for Identity configurations
                modelBuilder.Entity<UserInfoModel>()
           .HasKey(u => u.UserId);

                modelBuilder.Entity<UserInfoModel>()
                    .Property(u => u.UserId)
                    .HasMaxLength(450); // Match IdentityUser.Id length 
            // Configure LockerReservations table
            modelBuilder.Entity<LockerReservationModel>(entity =>
            {
                entity.ToTable("LockerReservations");
                entity.Property(e => e.Status)
                    .HasDefaultValue("Pending")
                    .HasMaxLength(50);

                entity.Property(e => e.LockerNumber)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.UserId)
                    .IsRequired();

                entity.Property(e => e.UserEmail)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.Purpose)
                    .HasMaxLength(500);
            });

            // Configure Expenses table if you have it
            // modelBuilder.Entity<Expense>(entity => { ... });

            // Configure ActivityReservations table if you have it  
            // modelBuilder.Entity<ActivityReservation>(entity => { ... });

            // Configure GatePasses table if you have it
            // modelBuilder.Entity<GatePass>(entity => { ... });
        }

    }
}
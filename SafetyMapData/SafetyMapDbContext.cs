using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SafetyMapData.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafetyMapData
{
    public class SafetyMapDbContext : IdentityDbContext<UserIdentity>  //TUK CHATA POMAGA ;)
    {
        public SafetyMapDbContext(DbContextOptions<SafetyMapDbContext> options)
        : base(options)
        {
        }

        // 1. Register your tables
        public DbSet<City> Cities { get; set; }
        public DbSet<Neighborhood> Neighborhoods { get; set; }
        public DbSet<CrimeCategory> CrimeCategories { get; set; }
        public DbSet<CrimeStatistic> CrimeStatistics { get; set; }
        public DbSet<UserSubscription> UserSubscriptions { get; set; }
        public DbSet<UserIdentity> ApplicationUsers { get; set; }
        // 2. Configure relationships and rules
        protected override void OnModelCreating(ModelBuilder builder)
        {
            // 1. IMPORTANT: This configures all the Identity tables (AspNetUsers, etc.)
            base.OnModelCreating(builder);

            // --- Configuration for UserSubscription (Many-to-Many) ---

            // A. Composite Key (Optional but good practice for M:N)
            // This makes sure the "UserId" + "NeighborhoodId" combo is the actual unique identifier
            // Note: Since you have an "Id" column in UserSubscription, this is just an Index.
            builder.Entity<UserSubscription>()
                .HasIndex(us => new { us.UserId, us.NeighborhoodId })
                .IsUnique();

            // B. Relationship: User <-> UserSubscription
            // *NEW* - This is the part you asked for
            builder.Entity<UserSubscription>()
                .HasOne(us => us.User)
                .WithMany(u => u.Subscriptions)
                .HasForeignKey(us => us.UserId)
                .OnDelete(DeleteBehavior.Cascade); // If User is deleted, delete their subscriptions

            // C. Relationship: Neighborhood <-> UserSubscription
            builder.Entity<UserSubscription>()
                .HasOne(us => us.Neighborhood)
                .WithMany(n => n.Subscribers)
                .HasForeignKey(us => us.NeighborhoodId)
                .OnDelete(DeleteBehavior.Cascade); // If Neighborhood is deleted, delete subscriptions

            // --- Configuration for CrimeStatistic (Many-to-Many) ---

            // Ensure unique stats (One record per Neighborhood + Category + Year)
            builder.Entity<CrimeStatistic>()
                .HasIndex(cs => new { cs.NeighborhoodId, cs.CrimeCategoryId, cs.Year })
                .IsUnique();

            // Relationship: Neighborhood <-> CrimeStatistic
            builder.Entity<CrimeStatistic>()
                .HasOne(cs => cs.Neighborhood)
                .WithMany(n => n.CrimeStatistics)
                .HasForeignKey(cs => cs.NeighborhoodId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relationship: CrimeCategory <-> CrimeStatistic
            builder.Entity<CrimeStatistic>()
                .HasOne(cs => cs.CrimeCategory)
                .WithMany(cc => cc.Statistics)
                .HasForeignKey(cs => cs.CrimeCategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // --- Configuration for City ---

            // Relationship: City <-> Neighborhood
            builder.Entity<Neighborhood>()
                .HasOne(n => n.City)
                .WithMany(c => c.Neighborhoods)
                .HasForeignKey(n => n.CityId)
                .OnDelete(DeleteBehavior.Cascade);
        }

    }
}

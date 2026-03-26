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
    public class SafetyMapDbContext : IdentityDbContext<UserIdentity> 
    {
        public SafetyMapDbContext(DbContextOptions<SafetyMapDbContext> options)
        : base(options)
        {
        }

        public DbSet<City> Cities { get; set; }
        public DbSet<Neighborhood> Neighborhoods { get; set; }
        public DbSet<CrimeCategory> CrimeCategories { get; set; }
        public DbSet<CrimeStatistic> CrimeStatistics { get; set; }
        public DbSet<UserSubscription> UserSubscriptions { get; set; }
        public DbSet<UserIdentity> ApplicationUsers { get; set; }
        public DbSet<UserCrimeReport> UserCrimeReports { get; set; }
        public DbSet<UserCrimeReportImage> UserCrimeReportImages { get; set; }
        // 2. Configure relationships and rules
        protected override void OnModelCreating(ModelBuilder builder)
        {
            // 1. IMPORTANT: This configures all the Identity tables (AspNetUsers, etc.)
            base.OnModelCreating(builder);

            builder.ApplyConfigurationsFromAssembly(typeof(SafetyMapDbContext).Assembly);
        }

    }
}

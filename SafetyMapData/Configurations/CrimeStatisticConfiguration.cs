using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafetyMapData.Entities;

namespace SafetyMapData.Configurations
{
    public class CrimeStatisticConfiguration : IEntityTypeConfiguration<CrimeStatistic>
    {
        public void Configure(EntityTypeBuilder<CrimeStatistic> builder)
        {
            // Ensure unique stats (One record per Neighborhood + Category + Year)
            builder.HasIndex(cs => new { cs.NeighborhoodId, cs.CrimeCategoryId, cs.Year })
                .IsUnique();

            // Relationship: Neighborhood <-> CrimeStatistic
            builder.HasOne(cs => cs.Neighborhood)
                .WithMany(n => n.CrimeStatistics)
                .HasForeignKey(cs => cs.NeighborhoodId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relationship: CrimeCategory <-> CrimeStatistic
            builder.HasOne(cs => cs.CrimeCategory)
                .WithMany(cc => cc.CrimeStatistics)
                .HasForeignKey(cs => cs.CrimeCategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafetyMapData.Entities;

namespace SafetyMapData.Configurations
{
    public class UserCrimeReportConfiguration : IEntityTypeConfiguration<UserCrimeReport>
    {
        public void Configure(EntityTypeBuilder<UserCrimeReport> builder)
        {
            builder.HasKey(r => r.Id);

            builder.Property(r => r.Description)
                .IsRequired()
                .HasMaxLength(1500);

            builder.Property(r => r.DateOfIncident)
                .IsRequired();

            // Relationships
            builder.HasOne(r => r.CrimeCategory)
                .WithMany()
                .HasForeignKey(r => r.CrimeCategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(r => r.City)
                .WithMany()
                .HasForeignKey(r => r.CityId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.Neighborhood)
                .WithMany()
                .HasForeignKey(r => r.NeighborhoodId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(r => r.UserIdentity)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafetyMapData.Entities;

namespace SafetyMapData.Configurations
{
    public class UserSubscriptionConfiguration : IEntityTypeConfiguration<UserSubscription>
    {
        public void Configure(EntityTypeBuilder<UserSubscription> builder)
        {
            // A. Composite Key (Optional but good practice for M:N)
            // This makes sure the "UserId" + "NeighborhoodId" combo is the actual unique identifier
            // Note: Since you have an "Id" column in UserSubscription, this is just an Index.
            builder.HasIndex(us => new { us.UserId, us.NeighborhoodId })
                .IsUnique();

            // B. Relationship: User <-> UserSubscription
            // *NEW* - This is the part you asked for
            builder.HasOne(us => us.User)
                .WithMany(u => u.Subscriptions)
                .HasForeignKey(us => us.UserId)
                .OnDelete(DeleteBehavior.Cascade); // If User is deleted, delete their subscriptions

            // C. Relationship: Neighborhood <-> UserSubscription
            builder.HasOne(us => us.Neighborhood)
                .WithMany(n => n.UserSubscriptions)
                .HasForeignKey(us => us.NeighborhoodId)
                .OnDelete(DeleteBehavior.Cascade); // If Neighborhood is deleted, delete subscriptions
        }
    }
}

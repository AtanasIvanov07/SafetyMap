using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using SafetyMapData;
using SafetyMapData.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SafetyMapWeb.Seeding.Seeders
{
    public static class UserSubscriptionSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SafetyMapDbContext>();

            if (await context.UserSubscriptions.AnyAsync())
            {
                return;
            }

            // Maria & Georgi
            var userId1 = "11111111-1111-1111-1111-111111111111";
            var userId2 = "22222222-2222-2222-2222-222222222222";

            var subscriptions = new List<UserSubscription>
            {
               new UserSubscription
               {
                   Id = Guid.NewGuid(),
                   UserId = userId1,
                   NeighborhoodId = Guid.Parse("N1111111-1111-1111-1111-111111111111"), // Mladost
                   SubscribedAt = DateTime.UtcNow.AddDays(-10)
               },
               new UserSubscription
               {
                   Id = Guid.NewGuid(),
                   UserId = userId1,
                   NeighborhoodId = Guid.Parse("N2222221-2222-2222-2222-222222222222"), // Trakia
                   SubscribedAt = DateTime.UtcNow.AddDays(-2)
               },
               new UserSubscription
               {
                   Id = Guid.NewGuid(),
                   UserId = userId2,
                   NeighborhoodId = Guid.Parse("N3333332-3333-3333-3333-333333333333"), // Chaika
                   SubscribedAt = DateTime.UtcNow.AddDays(-5)
               }
            };

            await context.UserSubscriptions.AddRangeAsync(subscriptions);
            await context.SaveChangesAsync();
            Console.WriteLine("Seeded User Subscriptions.");
        }
    }
}

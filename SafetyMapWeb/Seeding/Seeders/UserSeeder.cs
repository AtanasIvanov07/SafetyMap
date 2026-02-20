using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SafetyMapData.Entities;
using System;
using System.Threading.Tasks;

namespace SafetyMapWeb.Seeding.Seeders
{
    public static class UserSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<UserIdentity>>();

            // Admin
            var adminUser = await userManager.FindByEmailAsync("admin@safetymap.com");
            if (adminUser == null)
            {
                var user = new UserIdentity
                {
                    UserName = "admin@safetymap.com",
                    Email = "admin@safetymap.com",
                    FirstName = "System",
                    LastName = "Admin",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, "Admin123!"); // Password is Admin123!
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Admin");
                    Console.WriteLine("Seeded Admin user.");
                }
            }

            //user
            var testUser1 = await userManager.FindByEmailAsync("maria@safetymap.com");
            if (testUser1 == null)
            {
                var user1 = new UserIdentity
                {
                    Id = "11111111-1111-1111-1111-111111111111", // Hardcoded Id for seed references
                    UserName = "maria@safetymap.com",
                    Email = "maria@safetymap.com",
                    FirstName = "Maria",
                    LastName = "Ivanova",
                    EmailConfirmed = true
                };
                var result1 = await userManager.CreateAsync(user1, "User123!");
                if (result1.Succeeded)
                {
                    await userManager.AddToRoleAsync(user1, "User");
                }
            }

            //user
            var testUser2 = await userManager.FindByEmailAsync("georgi@safetymap.com");
            if (testUser2 == null)
            {
                var user2 = new UserIdentity
                {
                    Id = "22222222-2222-2222-2222-222222222222", // Hardcoded Id for seed references
                    UserName = "georgi@safetymap.com",
                    Email = "georgi@safetymap.com",
                    FirstName = "Georgi",
                    LastName = "Petrov",
                    EmailConfirmed = true
                };
                var result2 = await userManager.CreateAsync(user2, "User123!");
                if (result2.Succeeded)
                {
                    await userManager.AddToRoleAsync(user2, "User");
                }
            }
        }
    }
}

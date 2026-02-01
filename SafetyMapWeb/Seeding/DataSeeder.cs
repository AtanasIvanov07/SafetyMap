using Microsoft.AspNetCore.Identity;
using SafetyMapData.Entities;
using SafetyMapData;
using Microsoft.EntityFrameworkCore;
namespace SafetyMapWeb.Seeding
{
    public static class DataSeeder
    {
        public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string[] roleNames = { "Admin", "User" };

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                    Console.WriteLine($"Seeded role: {roleName}");
                }
            }
        }

        public static async Task SeedCitiesAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SafetyMapDbContext>();

            if (await context.Cities.AnyAsync())
            {
                return;
            }

            var cities = new List<City>
            {
               new City { Id = Guid.NewGuid(), Name = "Sofia", Population = 1241675 },
               new City { Id = Guid.NewGuid(), Name = "Plovdiv", Population = 346893 },
               new City { Id = Guid.NewGuid(), Name = "Varna", Population = 336505 },
               new City { Id = Guid.NewGuid(), Name = "Burgas", Population = 202434 },
               new City { Id = Guid.NewGuid(), Name = "Ruse", Population = 142902 },
               new City { Id = Guid.NewGuid(), Name = "Stara Zagora", Population = 136307 },
               new City { Id = Guid.NewGuid(), Name = "Pleven", Population = 96630 },
               new City { Id = Guid.NewGuid(), Name = "Blagoevgrad", Population = 69610 },
               new City { Id = Guid.NewGuid(), Name = "Dobrich", Population = 83584 },
               new City { Id = Guid.NewGuid(), Name = "Gabrovo", Population = 52169 },
               new City { Id = Guid.NewGuid(), Name = "Haskovo", Population = 71214 },
               new City { Id = Guid.NewGuid(), Name = "Kardzhali", Population = 49880 },
               new City { Id = Guid.NewGuid(), Name = "Kyustendil", Population = 40733 },
               new City { Id = Guid.NewGuid(), Name = "Lovech", Population = 32238 },
               new City { Id = Guid.NewGuid(), Name = "Montana", Population = 39712 },
               new City { Id = Guid.NewGuid(), Name = "Pazardzhik", Population = 69365 },
               new City { Id = Guid.NewGuid(), Name = "Pernik", Population = 72456 },
               new City { Id = Guid.NewGuid(), Name = "Razgrad", Population = 29938 },
               new City { Id = Guid.NewGuid(), Name = "Shumen", Population = 75442 },
               new City { Id = Guid.NewGuid(), Name = "Silistra", Population = 30983 },
               new City { Id = Guid.NewGuid(), Name = "Sliven", Population = 86275 },
               new City { Id = Guid.NewGuid(), Name = "Smolyan", Population = 27092 },
               new City { Id = Guid.NewGuid(), Name = "Targovishte", Population = 35344 },
               new City { Id = Guid.NewGuid(), Name = "Veliko Tarnovo", Population = 66993 },
               new City { Id = Guid.NewGuid(), Name = "Vidin", Population = 40550 },
               new City { Id = Guid.NewGuid(), Name = "Vratsa", Population = 50285 },
               new City { Id = Guid.NewGuid(), Name = "Yambol", Population = 65421 }
            };

            await context.Cities.AddRangeAsync(cities);
            await context.SaveChangesAsync();
        }
    }


}


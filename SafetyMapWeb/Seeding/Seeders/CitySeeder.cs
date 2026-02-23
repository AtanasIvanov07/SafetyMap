using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using SafetyMapData;
using SafetyMapData.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SafetyMapWeb.Seeding.Seeders
{
    public static class CitySeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SafetyMapDbContext>();

    

            var cities = new List<City>
            {
               new City { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "Sofia", Population = 1241675 },
               new City { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "Plovdiv", Population = 346893 },
               new City { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Name = "Varna", Population = 336505 },
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

            foreach (var city in cities)
            {
                if (!await context.Cities.AnyAsync(c => c.Id == city.Id))
                {
                    await context.Cities.AddAsync(city);
                }
            }
            await context.SaveChangesAsync();
            Console.WriteLine("Seeded Cities.");
        }
    }
}

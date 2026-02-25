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
               new City { Id = Guid.Parse("44444444-4444-4444-4444-444444444444"), Name = "Burgas", Population = 202434 },
               new City { Id = Guid.Parse("55555555-5555-5555-5555-555555555555"), Name = "Ruse", Population = 142902 },
               new City { Id = Guid.Parse("66666666-6666-6666-6666-666666666666"), Name = "Stara Zagora", Population = 136307 },
               new City { Id = Guid.Parse("77777777-7777-7777-7777-777777777777"), Name = "Pleven", Population = 96630 },
               new City { Id = Guid.Parse("88888888-8888-8888-8888-888888888888"), Name = "Blagoevgrad", Population = 69610 },
               new City { Id = Guid.Parse("99999999-9999-9999-9999-999999999999"), Name = "Dobrich", Population = 83584 },
               new City { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Name = "Gabrovo", Population = 52169 },
               new City { Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), Name = "Haskovo", Population = 71214 },
               new City { Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"), Name = "Kardzhali", Population = 49880 },
               new City { Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"), Name = "Kyustendil", Population = 40733 },
               new City { Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), Name = "Lovech", Population = 32238 },
               new City { Id = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"), Name = "Montana", Population = 39712 },
               new City { Id = Guid.Parse("11112222-3333-4444-5555-111111111111"), Name = "Pazardzhik", Population = 69365 },
               new City { Id = Guid.Parse("11112222-3333-4444-5555-222222222222"), Name = "Pernik", Population = 72456 },
               new City { Id = Guid.Parse("11112222-3333-4444-5555-333333333333"), Name = "Razgrad", Population = 29938 },
               new City { Id = Guid.Parse("11112222-3333-4444-5555-444444444444"), Name = "Shumen", Population = 75442 },
               new City { Id = Guid.Parse("11112222-3333-4444-5555-555555555555"), Name = "Silistra", Population = 30983 },
               new City { Id = Guid.Parse("11112222-3333-4444-5555-666666666666"), Name = "Sliven", Population = 86275 },
               new City { Id = Guid.Parse("11112222-3333-4444-5555-777777777777"), Name = "Smolyan", Population = 27092 },
               new City { Id = Guid.Parse("11112222-3333-4444-5555-888888888888"), Name = "Targovishte", Population = 35344 },
               new City { Id = Guid.Parse("11112222-3333-4444-5555-999999999999"), Name = "Veliko Tarnovo", Population = 66993 },
               new City { Id = Guid.Parse("11112222-3333-4444-5555-aaaaaaaaaaaa"), Name = "Vidin", Population = 40550 },
               new City { Id = Guid.Parse("11112222-3333-4444-5555-bbbbbbbbbbbb"), Name = "Vratsa", Population = 50285 },
               new City { Id = Guid.Parse("11112222-3333-4444-5555-cccccccccccc"), Name = "Yambol", Population = 65421 }
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

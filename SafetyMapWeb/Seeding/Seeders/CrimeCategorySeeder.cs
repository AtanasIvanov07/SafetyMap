using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using SafetyMapData;
using SafetyMapData.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SafetyMapWeb.Seeding.Seeders
{
    public static class CrimeCategorySeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SafetyMapDbContext>();

           

            var categories = new List<CrimeCategory>
            {
               new CrimeCategory { Id = Guid.Parse("C1111111-1111-1111-1111-111111111111"), Name = "Theft", ColorCode = "#FFC107" },
               new CrimeCategory { Id = Guid.Parse("C2222222-2222-2222-2222-222222222222"), Name = "Robbery", ColorCode = "#FF9800" },
               new CrimeCategory { Id = Guid.Parse("C3333333-3333-3333-3333-333333333333"), Name = "Assault", ColorCode = "#F44336" },
               new CrimeCategory { Id = Guid.Parse("C4444444-4444-4444-4444-444444444444"), Name = "Homicide/Murder", ColorCode = "#B71C1C" },
               new CrimeCategory { Id = Guid.Parse("C5555555-5555-5555-5555-555555555555"), Name = "Vandalism", ColorCode = "#9C27B0" },
               new CrimeCategory { Id = Guid.Parse("C6666666-6666-6666-6666-666666666666"), Name = "Vehicle Theft", ColorCode = "#3F51B5" },
               new CrimeCategory { Id = Guid.Parse("C7777777-7777-7777-7777-777777777777"), Name = "Drug Offenses", ColorCode = "#4CAF50" }
            };

            foreach (var cat in categories)
            {
                if (!await context.CrimeCategories.AnyAsync(c => c.Id == cat.Id))
                {
                    await context.CrimeCategories.AddAsync(cat);
                }
            }
            await context.SaveChangesAsync();
            Console.WriteLine("Seeded Crime Categories.");
        }
    }
}

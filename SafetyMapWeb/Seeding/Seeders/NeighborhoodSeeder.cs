using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using SafetyMapData;
using SafetyMapData.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SafetyMapWeb.Seeding.Seeders
{
    public static class NeighborhoodSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SafetyMapDbContext>();

            // We removed the early exit so it always seeds missing defaults

            var sofiaId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var plovdivId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var varnaId = Guid.Parse("33333333-3333-3333-3333-333333333333");

            var neighborhoods = new List<Neighborhood>
            {
                // Sofia
                new Neighborhood { Id = Guid.Parse("A1111111-1111-1111-1111-111111111111"), Name = "Mladost", SafetyRating = 7, CityId = sofiaId },
                new Neighborhood { Id = Guid.Parse("A1111112-1111-1111-1111-111111111111"), Name = "Lyulin", SafetyRating = 5, CityId = sofiaId },
                new Neighborhood { Id = Guid.Parse("A1111113-1111-1111-1111-111111111111"), Name = "Lozenets", SafetyRating = 8, CityId = sofiaId },
                new Neighborhood { Id = Guid.Parse("A1111114-1111-1111-1111-111111111111"), Name = "Studentski Grad", SafetyRating = 4, CityId = sofiaId },

                // Plovdiv
                new Neighborhood { Id = Guid.Parse("A2222221-2222-2222-2222-222222222222"), Name = "Trakia", SafetyRating = 8, CityId = plovdivId },
                new Neighborhood { Id = Guid.Parse("A2222222-2222-2222-2222-222222222222"), Name = "Smirnenski", SafetyRating = 7, CityId = plovdivId },
                new Neighborhood { Id = Guid.Parse("A2222223-2222-2222-2222-222222222222"), Name = "Stolipinovo", SafetyRating = 2, CityId = plovdivId },
                new Neighborhood { Id = Guid.Parse("A2222224-2222-2222-2222-222222222222"), Name = "Kamenitsa", SafetyRating = 7, CityId = plovdivId },

                // Varna
                new Neighborhood { Id = Guid.Parse("A3333331-3333-3333-3333-333333333333"), Name = "Vladislavovo", SafetyRating = 6, CityId = varnaId },
                new Neighborhood { Id = Guid.Parse("A3333332-3333-3333-3333-333333333333"), Name = "Chaika", SafetyRating = 9, CityId = varnaId },
                new Neighborhood { Id = Guid.Parse("A3333333-3333-3333-3333-333333333333"), Name = "Asparuhovo", SafetyRating = 5, CityId = varnaId },
                new Neighborhood { Id = Guid.Parse("A3333334-3333-3333-3333-333333333333"), Name = "Briz", SafetyRating = 8, CityId = varnaId }
            };

            foreach (var neighborhood in neighborhoods)
            {
                if (!await context.Neighborhoods.AnyAsync(n => n.Id == neighborhood.Id))
                {
                    await context.Neighborhoods.AddAsync(neighborhood);
                }
            }
            await context.SaveChangesAsync();
            Console.WriteLine("Seeded Neighborhoods.");
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using SafetyMapData;
using SafetyMapData.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SafetyMapWeb.Seeding.Seeders
{
    public static class CrimeStatisticSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SafetyMapDbContext>();

          

            var random = new Random(42); // Seed for reproducible generation
            var stats = new List<CrimeStatistic>();
            var years = new[] { 2022, 2023 };

            // Fetch IDs mappings
            var categories = new[]
            {
                Guid.Parse("C1111111-1111-1111-1111-111111111111"), // Theft (High)
                Guid.Parse("C2222222-2222-2222-2222-222222222222"), // Robbery (Mid)
                Guid.Parse("C3333333-3333-3333-3333-333333333333"), // Assault (Mid)
                Guid.Parse("C4444444-4444-4444-4444-444444444444"), // Homicide (Very Low)
                Guid.Parse("C5555555-5555-5555-5555-555555555555"), // Vandalism (High)
                Guid.Parse("C6666666-6666-6666-6666-666666666666"), // Vehicle (Mid)
                Guid.Parse("C7777777-7777-7777-7777-777777777777")  // Drug (Mid-High)
            };

            if (await context.CrimeStatistics.AnyAsync())
            {
                var allStats = await context.CrimeStatistics.ToListAsync();
                context.CrimeStatistics.RemoveRange(allStats);
                await context.SaveChangesAsync();
            }

        
            var neighborhoods = await context.Neighborhoods.Include(n => n.City).ToListAsync();
            
      
            var neighborhoodCounts = neighborhoods.GroupBy(n => n.CityId).ToDictionary(g => g.Key, g => g.Count());

            foreach (var n in neighborhoods)
            {
               
                var populationScale = Math.Max(0.1, n.City.Population / 50000.0);
                var neighborhoodsInCity = neighborhoodCounts[n.CityId];
                
   
                var finalScale = populationScale / neighborhoodsInCity;

                foreach (var cId in categories)
                {
                    int lastYearCount = 0;
                    foreach (var year in years)
                    {
                        int baseCount = GetBaseRate(cId, random);
                        int scaledBaseCount = (int)(baseCount * finalScale);
        
                        int noise = (int)(scaledBaseCount * (random.NextDouble() * 0.3 - 0.15));
                        int currentCount = Math.Max(0, scaledBaseCount + noise);

                        double trend = 0;
                        if (year == 2023 && lastYearCount > 0)
                        {
                            trend = Math.Round(((double)(currentCount - lastYearCount) / lastYearCount) * 100, 2);
                        }

                        stats.Add(new CrimeStatistic
                        {
                            Id = Guid.NewGuid(),
                            NeighborhoodId = n.Id,
                            CrimeCategoryId = cId,
                            Year = year,
                            CountOfCrimes = currentCount,
                            TrendPercentage = trend
                        });

                        lastYearCount = currentCount;
                    }
                }
            }

            await context.CrimeStatistics.AddRangeAsync(stats);
            await context.SaveChangesAsync();
            Console.WriteLine("Seeded Crime Statistics.");
        }

        private static int GetBaseRate(Guid categoryId, Random random)
        {
            var idString = categoryId.ToString();
            if (idString.StartsWith("c111") || idString.StartsWith("c555")) return random.Next(100, 300); // Theft, Vandalism
            if (idString.StartsWith("c444")) return random.Next(0, 5);
            return random.Next(20, 80);
        }
    }
}

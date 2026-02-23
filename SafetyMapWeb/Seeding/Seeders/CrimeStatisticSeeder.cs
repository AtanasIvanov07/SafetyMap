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

            if (await context.CrimeStatistics.AnyAsync())
            {
                return;
            }

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

            var neighborhoodGuids = new[]
            {
                // Sofia
                Guid.Parse("A1111111-1111-1111-1111-111111111111"),
                Guid.Parse("A1111112-1111-1111-1111-111111111111"),
                Guid.Parse("A1111113-1111-1111-1111-111111111111"),
                Guid.Parse("A1111114-1111-1111-1111-111111111111"),
                // Plovdiv
                Guid.Parse("A2222221-2222-2222-2222-222222222222"),
                Guid.Parse("A2222222-2222-2222-2222-222222222222"),
                Guid.Parse("A2222223-2222-2222-2222-222222222222"),
                Guid.Parse("A2222224-2222-2222-2222-222222222222"),
                // Varna
                Guid.Parse("A3333331-3333-3333-3333-333333333333"),
                Guid.Parse("A3333332-3333-3333-3333-333333333333"),
                Guid.Parse("A3333333-3333-3333-3333-333333333333"),
                Guid.Parse("A3333334-3333-3333-3333-333333333333")
            };

            foreach (var nId in neighborhoodGuids)
            {
                foreach (var cId in categories)
                {
                    int lastYearCount = 0;
                    foreach (var year in years)
                    {
                        int baseCount = GetBaseRate(cId, random);


                        int currentCount = Math.Max(0, baseCount + random.Next(-10, 15));
                        double trend = 0;

                        if (year == 2023 && lastYearCount > 0)
                        {
                            trend = Math.Round(((double)(currentCount - lastYearCount) / lastYearCount) * 100, 2);
                        }

                        stats.Add(new CrimeStatistic
                        {
                            Id = Guid.NewGuid(),
                            NeighborhoodId = nId,
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

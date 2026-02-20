using Microsoft.AspNetCore.Identity;
using SafetyMapData;
using SafetyMapWeb.Seeding.Seeders;

namespace SafetyMapWeb.Seeding
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            await RoleSeeder.SeedAsync(serviceProvider);
            await CitySeeder.SeedAsync(serviceProvider);
            await UserSeeder.SeedAsync(serviceProvider);
            await CrimeCategorySeeder.SeedAsync(serviceProvider);
            await NeighborhoodSeeder.SeedAsync(serviceProvider);
            await CrimeStatisticSeeder.SeedAsync(serviceProvider);
            await UserSubscriptionSeeder.SeedAsync(serviceProvider);
        }
    }
}

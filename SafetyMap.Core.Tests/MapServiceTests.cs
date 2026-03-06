using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SafetyMap.Core.Services;
using SafetyMapData;
using SafetyMapData.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SafetyMap.Core.Tests.Services
{
    [TestFixture]
    public class MapServiceTests
    {
        private SafetyMapDbContext _context;
        private MapService _mapService;

        private Guid _cityId;
        private Guid _neighborhoodId;
        private Guid _categoryId1;
        private Guid _categoryId2;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<SafetyMapDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new SafetyMapDbContext(options);
            _mapService = new MapService(_context);

            _cityId = Guid.NewGuid();
            _neighborhoodId = Guid.NewGuid();
            _categoryId1 = Guid.NewGuid();
            _categoryId2 = Guid.NewGuid();
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        private async Task SeedDataAsync()
        {
            var city = new City { Id = _cityId, Name = "Test City", Population = 100000 };
            _context.Cities.Add(city);

            var neighborhood = new Neighborhood { Id = _neighborhoodId, Name = "Test Neighborhood", CityId = _cityId, SafetyRating = 5, City = city };
            _context.Neighborhoods.Add(neighborhood);

            var cat1 = new CrimeCategory { Id = _categoryId1, Name = "Theft", ColorCode = "#123" };
            var cat2 = new CrimeCategory { Id = _categoryId2, Name = "Assault", ColorCode = "#456" };
            _context.CrimeCategories.Add(cat1);
            _context.CrimeCategories.Add(cat2);

            _context.CrimeStatistics.Add(new CrimeStatistic { Id = Guid.NewGuid(), NeighborhoodId = _neighborhoodId, CrimeCategoryId = _categoryId1, CountOfCrimes = 10, Year = 2023, Neighborhood = neighborhood, CrimeCategory = cat1 });
            _context.CrimeStatistics.Add(new CrimeStatistic { Id = Guid.NewGuid(), NeighborhoodId = _neighborhoodId, CrimeCategoryId = _categoryId2, CountOfCrimes = 5, Year = 2023, Neighborhood = neighborhood, CrimeCategory = cat2 });
            _context.CrimeStatistics.Add(new CrimeStatistic { Id = Guid.NewGuid(), NeighborhoodId = _neighborhoodId, CrimeCategoryId = _categoryId1, CountOfCrimes = 15, Year = 2024, Neighborhood = neighborhood, CrimeCategory = cat1 });

            await _context.SaveChangesAsync();
        }

        [Test]
        public async Task GetPopulationDataAsync_ShouldReturnCityPopulations()
        {
            await SeedDataAsync();

            var result = await _mapService.GetPopulationDataAsync();

            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.First().Name, Is.EqualTo("Test City"));
            Assert.That(result.First().Population, Is.EqualTo(100000));
        }

        [Test]
        public async Task GetCrimeDataAsync_ShouldReturnAllCrimes_WhenNoFiltersApplied()
        {
            await SeedDataAsync();

            var result = await _mapService.GetCrimeDataAsync(null, null);

            Assert.That(result.Count(), Is.EqualTo(1)); // One city
            var cityCrime = result.First();
            Assert.That(cityCrime.CityName, Is.EqualTo("Test City"));
            Assert.That(cityCrime.TotalCrimes, Is.EqualTo(30)); // 10 + 5 + 15
            Assert.That(cityCrime.CrimesByCategory["Theft"], Is.EqualTo(25));
            Assert.That(cityCrime.CrimesByCategory["Assault"], Is.EqualTo(5));
        }

        [Test]
        public async Task GetCrimeDataAsync_ShouldReturnFilteredCrimes_WhenCategoryFilterApplied()
        {
            await SeedDataAsync();

            var result = await _mapService.GetCrimeDataAsync(_categoryId1, null);

            Assert.That(result.Count(), Is.EqualTo(1));
            var cityCrime = result.First();
            Assert.That(cityCrime.TotalCrimes, Is.EqualTo(25)); // 10 + 15
            Assert.That(cityCrime.CrimesByCategory.ContainsKey("Assault"), Is.False);
        }

        [Test]
        public async Task GetCrimeDataAsync_ShouldReturnFilteredCrimes_WhenYearFilterApplied()
        {
            await SeedDataAsync();

            var result = await _mapService.GetCrimeDataAsync(null, 2023);

            Assert.That(result.Count(), Is.EqualTo(1));
            var cityCrime = result.First();
            Assert.That(cityCrime.TotalCrimes, Is.EqualTo(15)); // 10 + 5
        }

        [Test]
        public async Task GetCrimeDataAsync_ShouldHandleMissingDataGracefully()
        {
            _context.Cities.Add(new City { Id = Guid.NewGuid(), Name = "Empty City", Population = 1000 });
            await _context.SaveChangesAsync();

            var result = await _mapService.GetCrimeDataAsync(null, null);

            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.First().TotalCrimes, Is.EqualTo(0));
            Assert.That(result.First().CrimesByCategory.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task GetCrimeCategoriesAsync_ShouldReturnCategories()
        {
            await SeedDataAsync();

            var result = await _mapService.GetCrimeCategoriesAsync();

            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.That(result.Any(c => c.Name == "Theft"), Is.True);
        }
    }
}

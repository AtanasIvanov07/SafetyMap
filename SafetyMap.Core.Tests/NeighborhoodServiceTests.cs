using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SafetyMap.Core.DTOs.Neighborhood;
using SafetyMap.Core.Services;
using SafetyMapData;
using SafetyMapData.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SafetyMap.Core.Tests.Services
{
    [TestFixture]
    public class NeighborhoodServiceTests
    {
        private SafetyMapDbContext _context;
        private NeighborhoodService _neighborhoodService;

        private Guid _city1Id;
        private Guid _city2Id;
        private Guid _neighborhoodId;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<SafetyMapDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new SafetyMapDbContext(options);
            _neighborhoodService = new NeighborhoodService(_context);

            _city1Id = Guid.NewGuid();
            _city2Id = Guid.NewGuid();
            _neighborhoodId = Guid.NewGuid();
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        private async Task SeedDataAsync()
        {
            var city1 = new City { Id = _city1Id, Name = "City 1", Population = 1000 };
            var city2 = new City { Id = _city2Id, Name = "City 2", Population = 2000 };
            
            _context.Cities.Add(city1);
            _context.Cities.Add(city2);

            _context.Neighborhoods.Add(new Neighborhood { Id = _neighborhoodId, Name = "Downtown", CityId = _city1Id, SafetyRating = 5, City = city1 });
            _context.Neighborhoods.Add(new Neighborhood { Id = Guid.NewGuid(), Name = "Uptown", CityId = _city1Id, SafetyRating = 3, City = city1 });
            _context.Neighborhoods.Add(new Neighborhood { Id = Guid.NewGuid(), Name = "Suburb", CityId = _city2Id, SafetyRating = 1, City = city2 });

            await _context.SaveChangesAsync();
        }

        [Test]
        public async Task GetAllAsync_ShouldReturnAllNeighborhoods()
        {
            await SeedDataAsync();

            var result = await _neighborhoodService.GetAllAsync();

            Assert.That(result.Count(), Is.EqualTo(3));
            Assert.That(result.All(n => n.CityName != "N/A"), Is.True);
        }

        [Test]
        public async Task GetByIdAsync_ShouldReturnNeighborhood_WhenExists()
        {
            await SeedDataAsync();

            var result = await _neighborhoodService.GetByIdAsync(_neighborhoodId);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(_neighborhoodId));
            Assert.That(result.Name, Is.EqualTo("Downtown"));
        }

        [Test]
        public async Task GetByIdAsync_ShouldReturnNull_WhenDoesNotExist()
        {
            var result = await _neighborhoodService.GetByIdAsync(Guid.NewGuid());

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task ExistsAsync_ShouldReturnTrue_WhenNameAndCityIdMatch()
        {
            await SeedDataAsync();

            var result = await _neighborhoodService.ExistsAsync("Downtown", _city1Id);

            Assert.That(result, Is.True);
        }

        [Test]
        public async Task ExistsAsync_ShouldReturnFalse_WhenNameMatchesButCityIdDoesNot()
        {
            await SeedDataAsync();

            var result = await _neighborhoodService.ExistsAsync("Downtown", _city2Id);

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task ExistsAsync_ShouldReturnFalse_WhenExcludingSelf()
        {
            await SeedDataAsync();

            var result = await _neighborhoodService.ExistsAsync("Downtown", _city1Id, excludeId: _neighborhoodId);

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task CreateAsync_ShouldAddNeighborhoodToDatabase()
        {
            var dto = new NeighborhoodCreateDTO { Name = "New Neighborhood", CityId = _city1Id, SafetyRating = 4 };

            await _neighborhoodService.CreateAsync(dto);

            var neighborhoodInDb = await _context.Neighborhoods.FirstOrDefaultAsync(n => n.Name == "New Neighborhood");
            Assert.That(neighborhoodInDb, Is.Not.Null);
            Assert.That(neighborhoodInDb!.CityId, Is.EqualTo(_city1Id));
        }

        [Test]
        public async Task UpdateAsync_ShouldModifyExistingNeighborhood()
        {
            await SeedDataAsync();
            var updateDto = new NeighborhoodEditDTO { Id = _neighborhoodId, Name = "Updated Downtown", CityId = _city2Id, SafetyRating = 2 };

            await _neighborhoodService.UpdateAsync(updateDto);

            var neighborhoodInDb = await _context.Neighborhoods.FindAsync(_neighborhoodId);
            Assert.That(neighborhoodInDb!.Name, Is.EqualTo("Updated Downtown"));
            Assert.That(neighborhoodInDb.CityId, Is.EqualTo(_city2Id));
        }

        [Test]
        public void UpdateAsync_ShouldThrowException_WhenDoesNotExist()
        {
            var updateDto = new NeighborhoodEditDTO { Id = Guid.NewGuid(), Name = "Name", CityId = Guid.NewGuid(), SafetyRating = 1 };

            var ex = Assert.ThrowsAsync<ArgumentException>(async () => await _neighborhoodService.UpdateAsync(updateDto));
            Assert.That(ex.Message, Does.Contain("was not found"));
        }

        [Test]
        public async Task DeleteAsync_ShouldRemoveNeighborhoodFromDatabase()
        {
            await SeedDataAsync();

            await _neighborhoodService.DeleteAsync(_neighborhoodId);

            var neighborhoodInDb = await _context.Neighborhoods.FindAsync(_neighborhoodId);
            Assert.That(neighborhoodInDb, Is.Null);
        }
    }
}

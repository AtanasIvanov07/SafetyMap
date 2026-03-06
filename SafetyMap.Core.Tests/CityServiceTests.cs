using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SafetyMap.Core.DTOs.City;
using SafetyMap.Core.Services;
using SafetyMapData;
using SafetyMapData.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SafetyMap.Core.Tests
{
    [TestFixture]
    public class CityServiceTests
    {
        private SafetyMapDbContext _context;
        private CityService _cityService;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<SafetyMapDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new SafetyMapDbContext(options);
            _cityService = new CityService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public async Task GetAllAsync_ShouldReturnAllCities()
        {
            // Arrange
            _context.Cities.Add(new City { Id = Guid.NewGuid(), Name = "City A", Population = 1000 });
            _context.Cities.Add(new City { Id = Guid.NewGuid(), Name = "City B", Population = 2000 });
            await _context.SaveChangesAsync();

            // Act
            var result = await _cityService.GetAllAsync();

            // Assert
            Assert.That(result.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task GetByIdAsync_ShouldReturnCity_WhenCityExists()
        {
            // Arrange
            var cityId = Guid.NewGuid();
            _context.Cities.Add(new City { Id = cityId, Name = "Test City", Population = 5000 });
            await _context.SaveChangesAsync();

            // Act
            var result = await _cityService.GetByIdAsync(cityId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(cityId));
            Assert.That(result.Name, Is.EqualTo("Test City"));
        }

        [Test]
        public async Task GetByIdAsync_ShouldReturnNull_WhenCityDoesNotExist()
        {
            // Act
            var result = await _cityService.GetByIdAsync(Guid.NewGuid());

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task CreateAsync_ShouldAddCityToDatabase()
        {
            // Arrange
            var dto = new CityCreateDTO { Name = "New City", Population = 10000 };

            // Act
            await _cityService.CreateAsync(dto);

            // Assert
            var cityInDb = await _context.Cities.FirstOrDefaultAsync(c => c.Name == "New City");
            Assert.That(cityInDb, Is.Not.Null);
            Assert.That(cityInDb!.Population, Is.EqualTo(10000));
        }

        [Test]
        public async Task UpdateAsync_ShouldModifyExistingCity()
        {
            // Arrange
            var cityId = Guid.NewGuid();
            _context.Cities.Add(new City { Id = cityId, Name = "Old Name", Population = 1000 });
            await _context.SaveChangesAsync();

            var updateDto = new CityEditDTO { Id = cityId, Name = "New Name", Population = 2000 };

            // Act
            await _cityService.UpdateAsync(updateDto);

            // Assert
            var cityInDb = await _context.Cities.FindAsync(cityId);
            Assert.That(cityInDb!.Name, Is.EqualTo("New Name"));
            Assert.That(cityInDb.Population, Is.EqualTo(2000));
        }

        [Test]
        public void UpdateAsync_ShouldThrowException_WhenCityDoesNotExist()
        {
            // Arrange
            var updateDto = new CityEditDTO { Id = Guid.NewGuid(), Name = "Name", Population = 1000 };

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () => await _cityService.UpdateAsync(updateDto));
            Assert.That(ex.Message, Does.Contain("was not found"));
        }

        [Test]
        public async Task DeleteAsync_ShouldRemoveCityFromDatabase()
        {
            // Arrange
            var cityId = Guid.NewGuid();
            _context.Cities.Add(new City { Id = cityId, Name = "City To Delete", Population = 1000 });
            await _context.SaveChangesAsync();

            // Act
            await _cityService.DeleteAsync(cityId);

            // Assert
            var cityInDb = await _context.Cities.FindAsync(cityId);
            Assert.That(cityInDb, Is.Null);
        }
    }
}

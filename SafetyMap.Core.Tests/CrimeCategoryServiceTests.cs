using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SafetyMap.Core.DTOs.CrimeCategory;
using SafetyMap.Core.Services;
using SafetyMapData;
using SafetyMapData.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SafetyMap.Core.Tests.Services
{
    [TestFixture]
    public class CrimeCategoryServiceTests
    {
        private SafetyMapDbContext _context;
        private CrimeCategoryService _crimeCategoryService;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<SafetyMapDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new SafetyMapDbContext(options);
            _crimeCategoryService = new CrimeCategoryService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public async Task GetAllAsync_ShouldReturnAllCrimeCategories()
        {
            // Arrange
            _context.CrimeCategories.Add(new CrimeCategory { Id = Guid.NewGuid(), Name = "Theft", ColorCode = "#FF0000" });
            _context.CrimeCategories.Add(new CrimeCategory { Id = Guid.NewGuid(), Name = "Assault", ColorCode = "#00FF00" });
            await _context.SaveChangesAsync();

            // Act
            var result = await _crimeCategoryService.GetAllAsync();

            // Assert
            Assert.That(result.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task GetByIdAsync_ShouldReturnCrimeCategory_WhenExists()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            _context.CrimeCategories.Add(new CrimeCategory { Id = categoryId, Name = "Vandalism", ColorCode = "#0000FF" });
            await _context.SaveChangesAsync();

            // Act
            var result = await _crimeCategoryService.GetByIdAsync(categoryId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(categoryId));
            Assert.That(result.Name, Is.EqualTo("Vandalism"));
            Assert.That(result.ColorCode, Is.EqualTo("#0000FF"));
        }

        [Test]
        public async Task GetByIdAsync_ShouldReturnNull_WhenDoesNotExist()
        {
            // Act
            var result = await _crimeCategoryService.GetByIdAsync(Guid.NewGuid());

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task CreateAsync_ShouldAddCrimeCategoryToDatabase()
        {
            // Arrange
            var dto = new CrimeCategoryCreateDTO { Name = "Arson", ColorCode = "#FFFF00" };

            // Act
            await _crimeCategoryService.CreateAsync(dto);

            // Assert
            var categoryInDb = await _context.CrimeCategories.FirstOrDefaultAsync(c => c.Name == "Arson");
            Assert.That(categoryInDb, Is.Not.Null);
            Assert.That(categoryInDb!.ColorCode, Is.EqualTo("#FFFF00"));
        }

        [Test]
        public async Task UpdateAsync_ShouldModifyExistingCrimeCategory()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            _context.CrimeCategories.Add(new CrimeCategory { Id = categoryId, Name = "Old Name", ColorCode = "#000000" });
            await _context.SaveChangesAsync();

            var updateDto = new CrimeCategoryEditDTO { Id = categoryId, Name = "New Name", ColorCode = "#FFFFFF" };

            // Act
            await _crimeCategoryService.UpdateAsync(updateDto);

            // Assert
            var categoryInDb = await _context.CrimeCategories.FindAsync(categoryId);
            Assert.That(categoryInDb!.Name, Is.EqualTo("New Name"));
            Assert.That(categoryInDb.ColorCode, Is.EqualTo("#FFFFFF"));
        }

        [Test]
        public void UpdateAsync_ShouldThrowException_WhenDoesNotExist()
        {
            // Arrange
            var updateDto = new CrimeCategoryEditDTO { Id = Guid.NewGuid(), Name = "Name", ColorCode = "#000" };

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () => await _crimeCategoryService.UpdateAsync(updateDto));
            Assert.That(ex.Message, Does.Contain("was not found"));
        }

        [Test]
        public async Task DeleteAsync_ShouldRemoveCrimeCategoryFromDatabase()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            _context.CrimeCategories.Add(new CrimeCategory { Id = categoryId, Name = "Category To Delete", ColorCode = "#123456" });
            await _context.SaveChangesAsync();

            // Act
            await _crimeCategoryService.DeleteAsync(categoryId);

            // Assert
            var categoryInDb = await _context.CrimeCategories.FindAsync(categoryId);
            Assert.That(categoryInDb, Is.Null);
        }
    }
}

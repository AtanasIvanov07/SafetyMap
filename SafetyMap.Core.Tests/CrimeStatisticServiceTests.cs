using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SafetyMap.Core.DTOs.CrimeStatistic;
using SafetyMap.Core.Services;
using SafetyMapData;
using SafetyMapData.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SafetyMap.Core.Tests.Services
{
    [TestFixture]
    public class CrimeStatisticServiceTests
    {
        private SafetyMapDbContext _context;
        private CrimeStatisticService _crimeStatisticService;

        private Guid _neighborhood1Id;
        private Guid _neighborhood2Id;
        private Guid _category1Id;
        private Guid _category2Id;
        private string _userId;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<SafetyMapDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new SafetyMapDbContext(options);
            _crimeStatisticService = new CrimeStatisticService(_context);

            _neighborhood1Id = Guid.NewGuid();
            _neighborhood2Id = Guid.NewGuid();
            _category1Id = Guid.NewGuid();
            _category2Id = Guid.NewGuid();
            _userId = Guid.NewGuid().ToString();
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        private async Task SeedDataAsync()
        {
            _context.Neighborhoods.Add(new Neighborhood { Id = _neighborhood1Id, Name = "Downtown", SafetyRating = 1 });
            _context.Neighborhoods.Add(new Neighborhood { Id = _neighborhood2Id, Name = "Uptown", SafetyRating = 2 });

            _context.CrimeCategories.Add(new CrimeCategory { Id = _category1Id, Name = "Theft", ColorCode = "#FF0000" });
            _context.CrimeCategories.Add(new CrimeCategory { Id = _category2Id, Name = "Assault", ColorCode = "#00FF00" });

            _context.CrimeStatistics.Add(new CrimeStatistic { Id = Guid.NewGuid(), NeighborhoodId = _neighborhood1Id, CrimeCategoryId = _category1Id, CountOfCrimes = 10, Year = 2023 });
            _context.CrimeStatistics.Add(new CrimeStatistic { Id = Guid.NewGuid(), NeighborhoodId = _neighborhood1Id, CrimeCategoryId = _category2Id, CountOfCrimes = 5, Year = 2023 });
            _context.CrimeStatistics.Add(new CrimeStatistic { Id = Guid.NewGuid(), NeighborhoodId = _neighborhood2Id, CrimeCategoryId = _category1Id, CountOfCrimes = 15, Year = 2023 });

            _context.UserSubscriptions.Add(new UserSubscription { Id = Guid.NewGuid(), UserId = _userId, NeighborhoodId = _neighborhood1Id });

            await _context.SaveChangesAsync();
        }

        [Test]
        public async Task GetAllAsync_ShouldReturnPagedAndFilteredResults()
        {
            await SeedDataAsync();

            // Act: Filter by "Downtown"
            var (stats1, total1) = await _crimeStatisticService.GetAllAsync(neighborhoodSearch: "Down");
            Assert.That(total1, Is.EqualTo(2));
            Assert.That(stats1.Count(), Is.EqualTo(2));

            // Act: Filter by category "Theft"
            var (stats2, total2) = await _crimeStatisticService.GetAllAsync(categorySearch: "Th");
            Assert.That(total2, Is.EqualTo(2));
            Assert.That(stats2.Count(), Is.EqualTo(2));

            // Act: Filter by year 2023
            var (stats3, total3) = await _crimeStatisticService.GetAllAsync(year: 2023);
            Assert.That(total3, Is.EqualTo(3));

            // Act: Pagination
            var (stats4, total4) = await _crimeStatisticService.GetAllAsync(currentPage: 1, itemsPerPage: 2);
            Assert.That(total4, Is.EqualTo(3));
            Assert.That(stats4.Count(), Is.EqualTo(2)); // Only 2 items returned
        }

        [Test]
        public async Task GetUserSubscribedStatisticsAsync_ShouldReturnSubscribedStats()
        {
            await SeedDataAsync();

            // Act
            var result = await _crimeStatisticService.GetUserSubscribedStatisticsAsync(_userId);

            // Assert
            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.That(result.All(r => r.NeighborhoodId == _neighborhood1Id), Is.True);
        }

        [Test]
        public async Task GetUserSubscribedStatisticsAsync_ShouldReturnEmpty_WhenNoSubscriptions()
        {
            await SeedDataAsync();

            // Act
            var result = await _crimeStatisticService.GetUserSubscribedStatisticsAsync(Guid.NewGuid().ToString());

            // Assert
            Assert.That(result.Count(), Is.EqualTo(0));
        }

        [Test]
        public async Task GetByIdAsync_ShouldReturnStatistic_WhenExists()
        {
            await SeedDataAsync();
            var statId = Guid.NewGuid();
            _context.CrimeStatistics.Add(new CrimeStatistic { Id = statId, NeighborhoodId = _neighborhood1Id, CrimeCategoryId = _category1Id, CountOfCrimes = 50, Year = 2022 });
            await _context.SaveChangesAsync();

            // Act
            var result = await _crimeStatisticService.GetByIdAsync(statId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(statId));
            Assert.That(result.CountOfCrimes, Is.EqualTo(50));
        }

        [Test]
        public async Task CreateAsync_ShouldAddStatisticToDatabase()
        {
            await SeedDataAsync();
            var dto = new CrimeStatisticCreateDTO { NeighborhoodId = _neighborhood1Id, CrimeCategoryId = _category1Id, CountOfCrimes = 100, Year = 2024 };

            // Act
            await _crimeStatisticService.CreateAsync(dto);

            // Assert
            var statInDb = await _context.CrimeStatistics.FirstOrDefaultAsync(c => c.CountOfCrimes == 100);
            Assert.That(statInDb, Is.Not.Null);
            Assert.That(statInDb!.Year, Is.EqualTo(2024));
        }

        [Test]
        public async Task UpdateAsync_ShouldModifyExistingStatistic()
        {
            await SeedDataAsync();
            var statId = Guid.NewGuid();
            _context.CrimeStatistics.Add(new CrimeStatistic { Id = statId, NeighborhoodId = _neighborhood1Id, CrimeCategoryId = _category1Id, CountOfCrimes = 10, Year = 2022 });
            await _context.SaveChangesAsync();

            var updateDto = new CrimeStatisticEditDTO { Id = statId, NeighborhoodId = _neighborhood2Id, CrimeCategoryId = _category2Id, CountOfCrimes = 20, Year = 2023 };

            // Act
            await _crimeStatisticService.UpdateAsync(updateDto);

            // Assert
            var statInDb = await _context.CrimeStatistics.FindAsync(statId);
            Assert.That(statInDb!.CountOfCrimes, Is.EqualTo(20));
            Assert.That(statInDb.Year, Is.EqualTo(2023));
        }

        [Test]
        public void UpdateAsync_ShouldThrowException_WhenDoesNotExist()
        {
            var updateDto = new CrimeStatisticEditDTO { Id = Guid.NewGuid(), NeighborhoodId = Guid.NewGuid(), CrimeCategoryId = Guid.NewGuid(), CountOfCrimes = 10, Year = 2022 };

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () => await _crimeStatisticService.UpdateAsync(updateDto));
            Assert.That(ex.Message, Does.Contain("was not found"));
        }

        [Test]
        public async Task DeleteAsync_ShouldRemoveStatisticFromDatabase()
        {
            await SeedDataAsync();
            var statId = Guid.NewGuid();
            _context.CrimeStatistics.Add(new CrimeStatistic { Id = statId, NeighborhoodId = _neighborhood1Id, CrimeCategoryId = _category1Id, CountOfCrimes = 10, Year = 2022 });
            await _context.SaveChangesAsync();

            // Act
            await _crimeStatisticService.DeleteAsync(statId);

            // Assert
            var statInDb = await _context.CrimeStatistics.FindAsync(statId);
            Assert.That(statInDb, Is.Null);
        }

        [Test]
        public async Task GetNeighborhoodSelectListAsync_ShouldReturnPairs()
        {
            await SeedDataAsync();

            var result = await _crimeStatisticService.GetNeighborhoodSelectListAsync();

            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.That(result.Any(r => r.Value == "Downtown"), Is.True);
        }

        [Test]
        public async Task GetCrimeCategorySelectListAsync_ShouldReturnPairs()
        {
            await SeedDataAsync();

            var result = await _crimeStatisticService.GetCrimeCategorySelectListAsync();

            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.That(result.Any(r => r.Value == "Theft"), Is.True);
        }
    }
}

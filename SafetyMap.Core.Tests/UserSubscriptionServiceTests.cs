using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SafetyMap.Core.DTOs.UserSubscription;
using SafetyMap.Core.Services;
using SafetyMapData;
using SafetyMapData.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SafetyMap.Core.Tests.Services
{
    [TestFixture]
    public class UserSubscriptionServiceTests
    {
        private SafetyMapDbContext _context;
        private UserSubscriptionService _subscriptionService;

        private string _userId1 = "user-1";
        private string _userId2 = "user-2";
        private Guid _neighborhood1Id = Guid.NewGuid();
        private Guid _neighborhood2Id = Guid.NewGuid();
        private Guid _subscription1Id = Guid.NewGuid();

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<SafetyMapDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new SafetyMapDbContext(options);
            _subscriptionService = new UserSubscriptionService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        private async Task SeedDataAsync()
        {
            _context.Users.Add(new UserIdentity { Id = _userId1, UserName = "UserOne", FirstName = "User", LastName = "One" });
            _context.Users.Add(new UserIdentity { Id = _userId2, UserName = "UserTwo", FirstName = "User", LastName = "Two" });

            _context.Neighborhoods.Add(new Neighborhood { Id = _neighborhood1Id, Name = "Neighborhood 1", SafetyRating = 1 });
            _context.Neighborhoods.Add(new Neighborhood { Id = _neighborhood2Id, Name = "Neighborhood 2", SafetyRating = 2 });

            _context.UserSubscriptions.Add(new UserSubscription { Id = _subscription1Id, UserId = _userId1, NeighborhoodId = _neighborhood1Id, SubscribedAt = DateTime.UtcNow });
            _context.UserSubscriptions.Add(new UserSubscription { Id = Guid.NewGuid(), UserId = _userId1, NeighborhoodId = _neighborhood2Id, SubscribedAt = DateTime.UtcNow });
            _context.UserSubscriptions.Add(new UserSubscription { Id = Guid.NewGuid(), UserId = _userId2, NeighborhoodId = _neighborhood1Id, SubscribedAt = DateTime.UtcNow });

            await _context.SaveChangesAsync();
        }

        [Test]
        public async Task GetAllAsync_ShouldReturnAllSubscriptions()
        {
            await SeedDataAsync();

            var result = await _subscriptionService.GetAllAsync();

            Assert.That(result.Count(), Is.EqualTo(3));
            Assert.That(result.Any(r => r.UserName == "UserOne"), Is.True);
        }

        [Test]
        public async Task GetByIdAsync_ShouldReturnSubscription_WhenExists()
        {
            await SeedDataAsync();

            var result = await _subscriptionService.GetByIdAsync(_subscription1Id);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(_subscription1Id));
            Assert.That(result.UserId, Is.EqualTo(_userId1));
        }

        [Test]
        public async Task GetByIdAsync_ShouldReturnNull_WhenDoesNotExist()
        {
            var result = await _subscriptionService.GetByIdAsync(Guid.NewGuid());

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetUserSubscriptionsAsync_ShouldReturnUsersSubscriptions()
        {
            await SeedDataAsync();

            var result = await _subscriptionService.GetUserSubscriptionsAsync(_userId1);

            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.That(result.All(r => r.UserId == _userId1), Is.True);
        }

        [Test]
        public async Task GetSubscriptionCountAsync_ShouldReturnCorrectCount()
        {
            await SeedDataAsync();

            var count = await _subscriptionService.GetSubscriptionCountAsync(_userId1);

            Assert.That(count, Is.EqualTo(2));
        }

        [Test]
        public async Task CreateAsync_ShouldAddSubscription_WhenUnderLimit()
        {
            var dto = new UserSubscriptionCreateDTO { UserId = "new-user", NeighborhoodId = _neighborhood1Id };

            await _subscriptionService.CreateAsync(dto);

            var subInDb = await _context.UserSubscriptions.FirstOrDefaultAsync(u => u.UserId == "new-user");
            Assert.That(subInDb, Is.Not.Null);
            Assert.That(subInDb!.NeighborhoodId, Is.EqualTo(_neighborhood1Id));
        }

        [Test]
        public async Task CreateAsync_ShouldThrowException_WhenLimitReached()
        {
            _context.UserSubscriptions.Add(new UserSubscription { Id = Guid.NewGuid(), UserId = _userId1, NeighborhoodId = Guid.NewGuid() });
            _context.UserSubscriptions.Add(new UserSubscription { Id = Guid.NewGuid(), UserId = _userId1, NeighborhoodId = Guid.NewGuid() });
            _context.UserSubscriptions.Add(new UserSubscription { Id = Guid.NewGuid(), UserId = _userId1, NeighborhoodId = Guid.NewGuid() });
            await _context.SaveChangesAsync();

            var dto = new UserSubscriptionCreateDTO { UserId = _userId1, NeighborhoodId = _neighborhood1Id };

            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await _subscriptionService.CreateAsync(dto));
            Assert.That(ex.Message, Does.Contain("maximum limit"));
        }

        [Test]
        public async Task UpdateAsync_ShouldModifyExistingSubscription()
        {
            await SeedDataAsync();
            var updateDto = new UserSubscriptionEditDTO { Id = _subscription1Id, UserId = _userId2, NeighborhoodId = _neighborhood2Id, SubscribedAt = DateTime.UtcNow };

            await _subscriptionService.UpdateAsync(updateDto);

            var subInDb = await _context.UserSubscriptions.FindAsync(_subscription1Id);
            Assert.That(subInDb!.UserId, Is.EqualTo(_userId2));
            Assert.That(subInDb.NeighborhoodId, Is.EqualTo(_neighborhood2Id));
        }

        [Test]
        public void UpdateAsync_ShouldThrowException_WhenDoesNotExist()
        {
            var updateDto = new UserSubscriptionEditDTO { Id = Guid.NewGuid(), UserId = "user", NeighborhoodId = Guid.NewGuid(), SubscribedAt = DateTime.UtcNow };

            var ex = Assert.ThrowsAsync<ArgumentException>(async () => await _subscriptionService.UpdateAsync(updateDto));
            Assert.That(ex.Message, Does.Contain("was not found"));
        }

        [Test]
        public async Task DeleteAsync_ShouldRemoveSubscriptionFromDatabase()
        {
            await SeedDataAsync();

            await _subscriptionService.DeleteAsync(_subscription1Id);

            var subInDb = await _context.UserSubscriptions.FindAsync(_subscription1Id);
            Assert.That(subInDb, Is.Null);
        }

        [Test]
        public async Task GetNeighborhoodSelectListAsync_ShouldReturnPairs()
        {
            await SeedDataAsync();

            var result = await _subscriptionService.GetNeighborhoodSelectListAsync();

            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.That(result.Any(r => r.Value == "Neighborhood 1"), Is.True);
        }
    }
}

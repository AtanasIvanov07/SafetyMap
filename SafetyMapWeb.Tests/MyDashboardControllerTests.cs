using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using NUnit.Framework;
using SafetyMap.Core.Contracts;
using SafetyMap.Core.DTOs.CrimeStatistic;
using SafetyMap.Core.DTOs.UserSubscription;
using SafetyMapWeb.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SafetyMapWeb.Tests.Controllers
{
    [TestFixture]
    public class MyDashboardControllerTests
    {
        private Mock<IUserSubscriptionService> _userSubscriptionServiceMock;
        private Mock<ICrimeStatisticService> _crimeStatisticServiceMock;
        private MyDashboardController _controller;

        [SetUp]
        public void SetUp()
        {
            _userSubscriptionServiceMock = new Mock<IUserSubscriptionService>();
            _crimeStatisticServiceMock = new Mock<ICrimeStatisticService>();
            _controller = new MyDashboardController(_userSubscriptionServiceMock.Object, _crimeStatisticServiceMock.Object);
            _controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            SetUser(null);
        }

        [TearDown]
        public void TearDown()
        {
            _controller?.Dispose();
        }

        private void SetUser(string? userId)
        {
            var claims = new List<Claim>();
            if (!string.IsNullOrWhiteSpace(userId))
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
            }

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthentication"))
                }
            };
        }

        [Test]
        public async Task Index_ShouldReturnUnauthorized_WhenUserIdIsMissing()
        {
            var result = await _controller.Index() as UnauthorizedResult;

            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task Index_ShouldReturnViewAndPopulateViewBag_WhenUserIdExists()
        {
            var userId = "user-1";
            var subscriptions = new List<UserSubscriptionDTO>
            {
                new UserSubscriptionDTO { Id = Guid.NewGuid(), UserId = userId, NeighborhoodName = "Center" },
                new UserSubscriptionDTO { Id = Guid.NewGuid(), UserId = userId, NeighborhoodName = "West" }
            };
            var statistics = new List<CrimeStatisticDTO>
            {
                new CrimeStatisticDTO { Id = Guid.NewGuid(), NeighborhoodName = "Center", CrimeCategoryName = "Theft", CountOfCrimes = 7, Year = 2024 }
            };
            var neighborhoods = new List<KeyValuePair<string, string>>
            {
                new(Guid.NewGuid().ToString(), "Center")
            };

            SetUser(userId);
            _userSubscriptionServiceMock.Setup(s => s.GetUserSubscriptionsAsync(userId)).ReturnsAsync(subscriptions);
            _crimeStatisticServiceMock.Setup(s => s.GetUserSubscribedStatisticsAsync(userId)).ReturnsAsync(statistics);
            _userSubscriptionServiceMock.Setup(s => s.GetNeighborhoodSelectListAsync()).ReturnsAsync(neighborhoods);

            var result = await _controller.Index() as ViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(((IEnumerable<UserSubscriptionDTO>)_controller.ViewBag.Subscriptions).Count(), Is.EqualTo(2));
            Assert.That(((IEnumerable<CrimeStatisticDTO>)_controller.ViewBag.Statistics).Single().CrimeCategoryName, Is.EqualTo("Theft"));
            Assert.That(((IEnumerable<KeyValuePair<string, string>>)_controller.ViewBag.NeighborhoodList).Single().Value, Is.EqualTo("Center"));
            Assert.That((int)_controller.ViewBag.SubscriptionCount, Is.EqualTo(2));
        }

        [Test]
        public async Task Subscribe_ShouldReturnUnauthorized_WhenUserIdIsMissing()
        {
            var result = await _controller.Subscribe(Guid.NewGuid()) as UnauthorizedResult;

            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task Subscribe_ShouldCreateSubscriptionAndSetSuccessMessage_WhenRequestSucceeds()
        {
            var userId = "user-1";
            var neighborhoodId = Guid.NewGuid();
            SetUser(userId);

            var result = await _controller.Subscribe(neighborhoodId) as RedirectToActionResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.ActionName, Is.EqualTo("Index"));
            Assert.That(_controller.TempData["SuccessMessage"], Is.EqualTo("Successfully subscribed to the neighborhood alerts."));
            _userSubscriptionServiceMock.Verify(
                s => s.CreateAsync(It.Is<UserSubscriptionCreateDTO>(dto => dto.UserId == userId && dto.NeighborhoodId == neighborhoodId)),
                Times.Once);
        }

        [Test]
        public async Task Subscribe_ShouldSetErrorMessage_WhenSubscriptionAlreadyExists()
        {
            var userId = "user-1";
            var neighborhoodId = Guid.NewGuid();
            SetUser(userId);
            _userSubscriptionServiceMock.Setup(s => s.CreateAsync(It.IsAny<UserSubscriptionCreateDTO>()))
                .ThrowsAsync(new InvalidOperationException("Already subscribed."));

            var result = await _controller.Subscribe(neighborhoodId) as RedirectToActionResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.ActionName, Is.EqualTo("Index"));
            Assert.That(_controller.TempData["ErrorMessage"], Is.EqualTo("Already subscribed."));
        }

        [Test]
        public async Task Subscribe_ShouldSetGenericErrorMessage_WhenUnexpectedExceptionOccurs()
        {
            var userId = "user-1";
            var neighborhoodId = Guid.NewGuid();
            SetUser(userId);
            _userSubscriptionServiceMock.Setup(s => s.CreateAsync(It.IsAny<UserSubscriptionCreateDTO>()))
                .ThrowsAsync(new Exception("Boom"));

            var result = await _controller.Subscribe(neighborhoodId) as RedirectToActionResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.ActionName, Is.EqualTo("Index"));
            Assert.That(_controller.TempData["ErrorMessage"], Is.EqualTo("An error occurred while attempting to subscribe."));
        }

        [Test]
        public async Task Unsubscribe_ShouldReturnUnauthorized_WhenUserIdIsMissing()
        {
            var result = await _controller.Unsubscribe(Guid.NewGuid()) as UnauthorizedResult;

            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task Unsubscribe_ShouldReturnNotFound_WhenSubscriptionDoesNotExist()
        {
            var userId = "user-1";
            SetUser(userId);
            _userSubscriptionServiceMock.Setup(s => s.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((UserSubscriptionDTO)null!);

            var result = await _controller.Unsubscribe(Guid.NewGuid()) as NotFoundResult;

            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task Unsubscribe_ShouldReturnNotFound_WhenSubscriptionBelongsToAnotherUser()
        {
            var userId = "user-1";
            var subscriptionId = Guid.NewGuid();
            SetUser(userId);
            _userSubscriptionServiceMock.Setup(s => s.GetByIdAsync(subscriptionId)).ReturnsAsync(new UserSubscriptionDTO
            {
                Id = subscriptionId,
                UserId = "other-user"
            });

            var result = await _controller.Unsubscribe(subscriptionId) as NotFoundResult;

            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task Unsubscribe_ShouldDeleteSubscriptionAndSetSuccessMessage_WhenSubscriptionBelongsToUser()
        {
            var userId = "user-1";
            var subscriptionId = Guid.NewGuid();
            SetUser(userId);
            _userSubscriptionServiceMock.Setup(s => s.GetByIdAsync(subscriptionId)).ReturnsAsync(new UserSubscriptionDTO
            {
                Id = subscriptionId,
                UserId = userId
            });

            var result = await _controller.Unsubscribe(subscriptionId) as RedirectToActionResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.ActionName, Is.EqualTo("Index"));
            Assert.That(_controller.TempData["SuccessMessage"], Is.EqualTo("Successfully unsubscribed."));
            _userSubscriptionServiceMock.Verify(s => s.DeleteAsync(subscriptionId), Times.Once);
        }
    }
}

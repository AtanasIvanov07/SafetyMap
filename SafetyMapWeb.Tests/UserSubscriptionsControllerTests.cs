using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SafetyMap.Core.Contracts;
using SafetyMap.Core.DTOs.UserSubscription;
using SafetyMapWeb.Controllers;
using SafetyMapWeb.Models.UserSubscriptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SafetyMapWeb.Tests.Controllers
{
    [TestFixture]
    public class UserSubscriptionsControllerTests
    {
        private Mock<IUserSubscriptionService> _userSubscriptionServiceMock;
        private UserSubscriptionsController _controller;

        [SetUp]
        public void SetUp()
        {
            _userSubscriptionServiceMock = new Mock<IUserSubscriptionService>();
            _controller = new UserSubscriptionsController(_userSubscriptionServiceMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _controller?.Dispose();
        }

        private static List<KeyValuePair<string, string>> CreateNeighborhoodSelectList()
        {
            return new List<KeyValuePair<string, string>>
            {
                new(Guid.NewGuid().ToString(), "Center"),
                new(Guid.NewGuid().ToString(), "West")
            };
        }

        [Test]
        public async Task Index_ShouldReturnViewWithMappedSubscriptions()
        {
            var subscriptions = new List<UserSubscriptionDTO>
            {
                new UserSubscriptionDTO { Id = Guid.NewGuid(), UserId = "u1", UserName = "User One", NeighborhoodName = "Center", SubscribedAt = new DateTime(2026, 1, 1) },
                new UserSubscriptionDTO { Id = Guid.NewGuid(), UserId = "u2", UserName = "User Two", NeighborhoodName = "West", SubscribedAt = new DateTime(2026, 1, 2) }
            };
            _userSubscriptionServiceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(subscriptions);

            var result = await _controller.Index() as ViewResult;

            Assert.That(result, Is.Not.Null);
            var model = result!.Model as List<UserSubscriptionIndexViewModel>;
            Assert.That(model, Is.Not.Null);
            Assert.That(model!.Count, Is.EqualTo(2));
            Assert.That(model[0].UserName, Is.EqualTo("User One"));
            Assert.That(model[1].NeighborhoodName, Is.EqualTo("West"));
        }

        [Test]
        public async Task Details_ShouldReturnNotFound_WhenIdIsNull()
        {
            var result = await _controller.Details(null) as NotFoundResult;

            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task Details_ShouldReturnNotFound_WhenSubscriptionDoesNotExist()
        {
            _userSubscriptionServiceMock.Setup(s => s.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((UserSubscriptionDTO)null!);

            var result = await _controller.Details(Guid.NewGuid()) as NotFoundResult;

            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task Details_ShouldReturnViewWithSubscription_WhenSubscriptionExists()
        {
            var subscriptionId = Guid.NewGuid();
            var subscription = new UserSubscriptionDTO { Id = subscriptionId, UserId = "u1", UserName = "User One", NeighborhoodName = "Center" };
            _userSubscriptionServiceMock.Setup(s => s.GetByIdAsync(subscriptionId)).ReturnsAsync(subscription);

            var result = await _controller.Details(subscriptionId) as ViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Model, Is.EqualTo(subscription));
        }

        // [Test]
        // public async Task Create_Get_ShouldReturnViewWithNeighborhoods()
        // {
        //     var neighborhoods = CreateNeighborhoodSelectList();
        //     _userSubscriptionServiceMock.Setup(s => s.GetNeighborhoodSelectListAsync()).ReturnsAsync(neighborhoods);

        //     var result = await _controller.Create() as ViewResult;

        //     Assert.That(result, Is.Not.Null);
        //     var model = result!.Model as UserSubscriptionCreateViewModel;
        //     Assert.That(model, Is.Not.Null);
        //     Assert.That(model!.Neighborhoods.Count(), Is.EqualTo(2));
        //     Assert.That(model.Neighborhoods.First().Text, Is.EqualTo("Center"));
        // }

        // [Test]
        // public async Task Create_Post_ShouldReturnViewAndRepopulateNeighborhoods_WhenModelStateIsInvalid()
        // {
        //     var model = new UserSubscriptionCreateViewModel { UserId = "u1", NeighborhoodId = Guid.NewGuid() };
        //     var neighborhoods = CreateNeighborhoodSelectList();
        //     _controller.ModelState.AddModelError("UserId", "Invalid");
        //     _userSubscriptionServiceMock.Setup(s => s.GetNeighborhoodSelectListAsync()).ReturnsAsync(neighborhoods);

        //     var result = await _controller.Create(model) as ViewResult;

        //     Assert.That(result, Is.Not.Null);
        //     Assert.That(result!.Model, Is.EqualTo(model));
        //     Assert.That(model.Neighborhoods.Count(), Is.EqualTo(2));
        //     _userSubscriptionServiceMock.Verify(s => s.CreateAsync(It.IsAny<UserSubscriptionCreateDTO>()), Times.Never);
        // }

        // [Test]
        // public async Task Create_Post_ShouldRedirectToIndex_WhenModelIsValid()
        // {
        //     var neighborhoodId = Guid.NewGuid();
        //     var model = new UserSubscriptionCreateViewModel { UserId = "u1", NeighborhoodId = neighborhoodId };

        //     var result = await _controller.Create(model) as RedirectToActionResult;

        //     Assert.That(result, Is.Not.Null);
        //     Assert.That(result!.ActionName, Is.EqualTo("Index"));
        //     _userSubscriptionServiceMock.Verify(
        //         s => s.CreateAsync(It.Is<UserSubscriptionCreateDTO>(dto => dto.UserId == model.UserId && dto.NeighborhoodId == neighborhoodId)),
        //         Times.Once);
        // }

        [Test]
        public async Task Edit_Get_ShouldReturnNotFound_WhenIdIsNull()
        {
            var result = await _controller.Edit(null) as NotFoundResult;

            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task Edit_Get_ShouldReturnNotFound_WhenSubscriptionDoesNotExist()
        {
            _userSubscriptionServiceMock.Setup(s => s.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((UserSubscriptionDTO)null!);

            var result = await _controller.Edit(Guid.NewGuid()) as NotFoundResult;

            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task Edit_Get_ShouldReturnViewWithModelAndNeighborhoods_WhenSubscriptionExists()
        {
            var subscriptionId = Guid.NewGuid();
            var subscribedAt = new DateTime(2026, 1, 1);
            var neighborhoods = CreateNeighborhoodSelectList();
            _userSubscriptionServiceMock.Setup(s => s.GetByIdAsync(subscriptionId)).ReturnsAsync(new UserSubscriptionDTO
            {
                Id = subscriptionId,
                UserId = "u1",
                NeighborhoodId = Guid.Parse(neighborhoods[1].Key),
                SubscribedAt = subscribedAt
            });
            _userSubscriptionServiceMock.Setup(s => s.GetNeighborhoodSelectListAsync()).ReturnsAsync(neighborhoods);

            var result = await _controller.Edit(subscriptionId) as ViewResult;

            Assert.That(result, Is.Not.Null);
            var model = result!.Model as UserSubscriptionEditViewModel;
            Assert.That(model, Is.Not.Null);
            Assert.That(model!.Id, Is.EqualTo(subscriptionId));
            Assert.That(model.SubscribedAt, Is.EqualTo(subscribedAt));
            Assert.That(model.Neighborhoods.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task Edit_Post_ShouldReturnNotFound_WhenIdMismatchesModelId()
        {
            var result = await _controller.Edit(Guid.NewGuid(), new UserSubscriptionEditViewModel { Id = Guid.NewGuid() }) as NotFoundResult;

            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task Edit_Post_ShouldReturnViewAndRepopulateNeighborhoods_WhenModelStateIsInvalid()
        {
            var neighborhoods = CreateNeighborhoodSelectList();
            var subscriptionId = Guid.NewGuid();
            var model = new UserSubscriptionEditViewModel
            {
                Id = subscriptionId,
                UserId = "u1",
                NeighborhoodId = Guid.Parse(neighborhoods[0].Key),
                SubscribedAt = new DateTime(2026, 1, 1)
            };
            _controller.ModelState.AddModelError("UserId", "Invalid");
            _userSubscriptionServiceMock.Setup(s => s.GetNeighborhoodSelectListAsync()).ReturnsAsync(neighborhoods);

            var result = await _controller.Edit(subscriptionId, model) as ViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Model, Is.EqualTo(model));
            Assert.That(model.Neighborhoods.Count(), Is.EqualTo(2));
            _userSubscriptionServiceMock.Verify(s => s.UpdateAsync(It.IsAny<UserSubscriptionEditDTO>()), Times.Never);
        }

        [Test]
        public async Task Edit_Post_ShouldRedirectToIndex_WhenModelIsValid()
        {
            var subscriptionId = Guid.NewGuid();
            var subscribedAt = new DateTime(2026, 1, 1);
            var neighborhoodId = Guid.NewGuid();
            var model = new UserSubscriptionEditViewModel
            {
                Id = subscriptionId,
                UserId = "u1",
                NeighborhoodId = neighborhoodId,
                SubscribedAt = subscribedAt
            };

            var result = await _controller.Edit(subscriptionId, model) as RedirectToActionResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.ActionName, Is.EqualTo("Index"));
            _userSubscriptionServiceMock.Verify(
                s => s.UpdateAsync(It.Is<UserSubscriptionEditDTO>(dto =>
                    dto.Id == subscriptionId &&
                    dto.UserId == model.UserId &&
                    dto.NeighborhoodId == neighborhoodId &&
                    dto.SubscribedAt == subscribedAt)),
                Times.Once);
        }

        [Test]
        public async Task Delete_ShouldReturnNotFound_WhenIdIsNull()
        {
            var result = await _controller.Delete(null) as NotFoundResult;

            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task Delete_ShouldReturnNotFound_WhenSubscriptionDoesNotExist()
        {
            _userSubscriptionServiceMock.Setup(s => s.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((UserSubscriptionDTO)null!);

            var result = await _controller.Delete(Guid.NewGuid()) as NotFoundResult;

            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task Delete_ShouldReturnViewWithSubscription_WhenSubscriptionExists()
        {
            var subscriptionId = Guid.NewGuid();
            var subscription = new UserSubscriptionDTO { Id = subscriptionId, UserId = "u1", UserName = "User One", NeighborhoodName = "Center" };
            _userSubscriptionServiceMock.Setup(s => s.GetByIdAsync(subscriptionId)).ReturnsAsync(subscription);

            var result = await _controller.Delete(subscriptionId) as ViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Model, Is.EqualTo(subscription));
        }

        [Test]
        public async Task DeleteConfirmed_ShouldRedirectToIndex()
        {
            var subscriptionId = Guid.NewGuid();

            var result = await _controller.DeleteConfirmed(subscriptionId) as RedirectToActionResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.ActionName, Is.EqualTo("Index"));
            _userSubscriptionServiceMock.Verify(s => s.DeleteAsync(subscriptionId), Times.Once);
        }
    }
}

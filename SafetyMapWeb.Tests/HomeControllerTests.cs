using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SafetyMap.Core.Contracts;
using SafetyMap.Core.DTOs.City;
using SafetyMap.Core.DTOs.CrimeStatistic;
using SafetyMap.Core.DTOs.Neighborhood;
using SafetyMap.Core.DTOs.UserSubscription;
using SafetyMapWeb.Controllers;
using SafetyMapWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SafetyMapWeb.Tests.Controllers
{
    [TestFixture]
    public class HomeControllerTests
    {
        private Mock<ILogger<HomeController>> _loggerMock;
        private Mock<ICityService> _cityServiceMock;
        private Mock<INeighborhoodService> _neighborhoodServiceMock;
        private Mock<ICrimeStatisticService> _crimeStatisticServiceMock;
        private Mock<IUserSubscriptionService> _userSubscriptionServiceMock;
        private HomeController _controller;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger<HomeController>>();
            _cityServiceMock = new Mock<ICityService>();
            _neighborhoodServiceMock = new Mock<INeighborhoodService>();
            _crimeStatisticServiceMock = new Mock<ICrimeStatisticService>();
            _userSubscriptionServiceMock = new Mock<IUserSubscriptionService>();

            _controller = new HomeController(
                _loggerMock.Object,
                _cityServiceMock.Object,
                _neighborhoodServiceMock.Object,
                _crimeStatisticServiceMock.Object,
                _userSubscriptionServiceMock.Object
            );
        }

        [TearDown]
        public void TearDown()
        {
            _controller?.Dispose();
        }

        [Test]
        public async Task Index_ShouldReturnViewWithViewBagCounts()
        {
            _cityServiceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<CityDTO> { new CityDTO(), new CityDTO() });
            _neighborhoodServiceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<NeighborhoodDTO> { new NeighborhoodDTO() });
            _crimeStatisticServiceMock.Setup(s => s.GetAllAsync(null, null, null, 1, 1)).ReturnsAsync((new List<CrimeStatisticDTO>(), 10));
            _userSubscriptionServiceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<UserSubscriptionDTO> { new UserSubscriptionDTO(), new UserSubscriptionDTO(), new UserSubscriptionDTO() });

            var result = await _controller.Index() as ViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(_controller.ViewBag.TotalCities, Is.EqualTo(2));
            Assert.That(_controller.ViewBag.TotalNeighborhoods, Is.EqualTo(1));
            Assert.That(_controller.ViewBag.TotalCrimeStatistics, Is.EqualTo(10));
            Assert.That(_controller.ViewBag.TotalActiveAlerts, Is.EqualTo(3));
        }

        [Test]
        public void Privacy_ShouldReturnView()
        {
            var result = _controller.Privacy() as ViewResult;
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void Error_ShouldReturnViewWithErrorViewModel()
        {
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            
            // Setting a mock TraceIdentifier to mimic HttpContext
            _controller.ControllerContext.HttpContext.TraceIdentifier = "test-trace-id";

            var result = _controller.Error() as ViewResult;

            Assert.That(result, Is.Not.Null);
            var model = result!.Model as ErrorViewModel;
            Assert.That(model, Is.Not.Null);
            Assert.That(model!.RequestId, Is.EqualTo("test-trace-id"));
        }
    }
}

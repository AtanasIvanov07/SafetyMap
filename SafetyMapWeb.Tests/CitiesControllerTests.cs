using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SafetyMap.Core.Contracts;
using SafetyMap.Core.DTOs.City;
using SafetyMapWeb.Controllers;
using SafetyMapWeb.Models.Cities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SafetyMapWeb.Tests.Controllers
{
    [TestFixture]
    public class CitiesControllerTests
    {
        private Mock<ICityService> _cityServiceMock;
        private CitiesController _controller;

        [SetUp]
        public void SetUp()
        {
            _cityServiceMock = new Mock<ICityService>();
            _controller = new CitiesController(_cityServiceMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _controller?.Dispose();
        }

        [Test]
        public async Task Index_ShouldReturnViewWithCities()
        {
            var cities = new List<CityDTO>
            {
                new CityDTO { Id = Guid.NewGuid(), Name = "City 1", Population = 1000 },
                new CityDTO { Id = Guid.NewGuid(), Name = "City 2", Population = 2000 }
            };
            _cityServiceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(cities);

            var result = await _controller.Index() as ViewResult;

            Assert.That(result, Is.Not.Null);
            var model = result!.Model as List<CityIndexViewModel>;
            Assert.That(model, Is.Not.Null);
            Assert.That(model!.Count, Is.EqualTo(2));
            Assert.That(model.Any(m => m.Name == "City 1"), Is.True);
        }

        [Test]
        public async Task Details_ShouldReturnNotFound_WhenIdIsNull()
        {
            var result = await _controller.Details(null) as NotFoundResult;
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task Details_ShouldReturnNotFound_WhenCityDoesNotExist()
        {
            _cityServiceMock.Setup(s => s.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((CityDTO)null!);

            var result = await _controller.Details(Guid.NewGuid()) as NotFoundResult;
            
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task Details_ShouldReturnViewWithCity_WhenCityExists()
        {
            var cityId = Guid.NewGuid();
            var city = new CityDTO { Id = cityId, Name = "City", Population = 1000 };
            _cityServiceMock.Setup(s => s.GetByIdAsync(cityId)).ReturnsAsync(city);

            var result = await _controller.Details(cityId) as ViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Model, Is.EqualTo(city));
        }



        [Test]
        public async Task Edit_Get_ShouldReturnNotFound_WhenIdIsNull()
        {
            var result = await _controller.Edit(null) as NotFoundResult;
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task Edit_Get_ShouldReturnNotFound_WhenCityDoesNotExist()
        {
            _cityServiceMock.Setup(s => s.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((CityDTO)null!);

            var result = await _controller.Edit(Guid.NewGuid()) as NotFoundResult;

            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task Edit_Get_ShouldReturnViewWithModel_WhenCityExists()
        {
            var cityId = Guid.NewGuid();
            var city = new CityDTO { Id = cityId, Name = "City", Population = 1000 };
            _cityServiceMock.Setup(s => s.GetByIdAsync(cityId)).ReturnsAsync(city);

            var result = await _controller.Edit(cityId) as ViewResult;

            Assert.That(result, Is.Not.Null);
            var model = result!.Model as CityEditViewModel;
            Assert.That(model, Is.Not.Null);
            Assert.That(model!.Id, Is.EqualTo(cityId));
            Assert.That(model.Name, Is.EqualTo("City"));
        }

        [Test]
        public async Task Edit_Post_ShouldReturnNotFound_WhenIdMismatchesModelId()
        {
            var model = new CityEditViewModel { Id = Guid.NewGuid() };
            var result = await _controller.Edit(Guid.NewGuid(), model) as NotFoundResult;

            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task Edit_Post_ShouldReturnView_WhenModelStateIsInvalid()
        {
            var cityId = Guid.NewGuid();
            var model = new CityEditViewModel { Id = cityId };
            _controller.ModelState.AddModelError("Error", "Invalid");

            var result = await _controller.Edit(cityId, model) as ViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Model, Is.EqualTo(model));
        }

        [Test]
        public async Task Edit_Post_ShouldRedirectToIndex_WhenValid()
        {
            var cityId = Guid.NewGuid();
            var model = new CityEditViewModel { Id = cityId, Name = "Updated", Population = 2000 };

            var result = await _controller.Edit(cityId, model) as RedirectToActionResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.ActionName, Is.EqualTo("Index"));
            _cityServiceMock.Verify(s => s.UpdateAsync(It.IsAny<CityEditDTO>()), Times.Once);
        }


    }
}

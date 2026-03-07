using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SafetyMap.Core.Contracts;
using SafetyMap.Core.DTOs.City;
using SafetyMap.Core.DTOs.Neighborhood;
using SafetyMapWeb.Controllers;
using SafetyMapWeb.Models.Neighborhoods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SafetyMapWeb.Tests.Controllers
{
    [TestFixture]
    public class NeighborhoodsControllerTests
    {
        private Mock<INeighborhoodService> _neighborhoodServiceMock;
        private Mock<ICityService> _cityServiceMock;
        private NeighborhoodsController _controller;

        [SetUp]
        public void SetUp()
        {
            _neighborhoodServiceMock = new Mock<INeighborhoodService>();
            _cityServiceMock = new Mock<ICityService>();
            _controller = new NeighborhoodsController(_neighborhoodServiceMock.Object, _cityServiceMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _controller?.Dispose();
        }

        private static List<CityDTO> CreateCities()
        {
            return new List<CityDTO>
            {
                new CityDTO { Id = Guid.NewGuid(), Name = "Sofia" },
                new CityDTO { Id = Guid.NewGuid(), Name = "Plovdiv" }
            };
        }

        [Test]
        public async Task Index_ShouldReturnViewWithMappedNeighborhoods()
        {
            var neighborhoods = new List<NeighborhoodDTO>
            {
                new NeighborhoodDTO { Id = Guid.NewGuid(), Name = "Center", SafetyRating = 8, CityName = "Sofia" },
                new NeighborhoodDTO { Id = Guid.NewGuid(), Name = "West", SafetyRating = 6, CityName = "Plovdiv" }
            };
            _neighborhoodServiceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(neighborhoods);

            var result = await _controller.Index() as ViewResult;

            Assert.That(result, Is.Not.Null);
            var model = result!.Model as List<NeighborhoodIndexViewModel>;
            Assert.That(model, Is.Not.Null);
            Assert.That(model!.Count, Is.EqualTo(2));
            Assert.That(model[0].Name, Is.EqualTo("Center"));
            Assert.That(model[1].CityName, Is.EqualTo("Plovdiv"));
        }

        [Test]
        public async Task Details_ShouldReturnNotFound_WhenIdIsNull()
        {
            var result = await _controller.Details(null) as NotFoundResult;

            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task Details_ShouldReturnNotFound_WhenNeighborhoodDoesNotExist()
        {
            _neighborhoodServiceMock.Setup(s => s.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((NeighborhoodDTO)null!);

            var result = await _controller.Details(Guid.NewGuid()) as NotFoundResult;

            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task Details_ShouldReturnViewWithNeighborhood_WhenNeighborhoodExists()
        {
            var neighborhoodId = Guid.NewGuid();
            var neighborhood = new NeighborhoodDTO { Id = neighborhoodId, Name = "Center", SafetyRating = 8, CityName = "Sofia" };
            _neighborhoodServiceMock.Setup(s => s.GetByIdAsync(neighborhoodId)).ReturnsAsync(neighborhood);

            var result = await _controller.Details(neighborhoodId) as ViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Model, Is.EqualTo(neighborhood));
        }

        [Test]
        public async Task Create_Get_ShouldReturnViewWithCities()
        {
            var cities = CreateCities();
            _cityServiceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(cities);

            var result = await _controller.Create() as ViewResult;

            Assert.That(result, Is.Not.Null);
            var model = result!.Model as NeighborhoodCreateViewModel;
            Assert.That(model, Is.Not.Null);
            Assert.That(model!.Cities.Count(), Is.EqualTo(2));
            Assert.That(model.Cities.First().Text, Is.EqualTo("Sofia"));
        }

        [Test]
        public async Task Create_Post_ShouldReturnViewAndRepopulateCities_WhenModelStateIsInvalid()
        {
            var model = new NeighborhoodCreateViewModel { Name = "Center", CityId = Guid.NewGuid(), SafetyRating = 8 };
            var cities = CreateCities();
            _controller.ModelState.AddModelError("Name", "Invalid");
            _cityServiceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(cities);

            var result = await _controller.Create(model) as ViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Model, Is.EqualTo(model));
            Assert.That(model.Cities.Count(), Is.EqualTo(2));
            _neighborhoodServiceMock.Verify(s => s.CreateAsync(It.IsAny<NeighborhoodCreateDTO>()), Times.Never);
        }

        [Test]
        public async Task Create_Post_ShouldReturnViewWithError_WhenNeighborhoodAlreadyExists()
        {
            var cities = CreateCities();
            var model = new NeighborhoodCreateViewModel { Name = "Center", CityId = cities[0].Id, SafetyRating = 8 };
            _neighborhoodServiceMock.Setup(s => s.ExistsAsync(model.Name, model.CityId, null)).ReturnsAsync(true);
            _cityServiceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(cities);

            var result = await _controller.Create(model) as ViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(_controller.ModelState["Name"]!.Errors[0].ErrorMessage, Is.EqualTo("A neighborhood with this name already exists in the selected city."));
            Assert.That(model.Cities.Count(), Is.EqualTo(2));
            _neighborhoodServiceMock.Verify(s => s.CreateAsync(It.IsAny<NeighborhoodCreateDTO>()), Times.Never);
        }

        [Test]
        public async Task Create_Post_ShouldRedirectToIndex_WhenModelIsValidAndNeighborhoodIsUnique()
        {
            var cityId = Guid.NewGuid();
            var model = new NeighborhoodCreateViewModel { Name = "Center", CityId = cityId, SafetyRating = 8 };
            _neighborhoodServiceMock.Setup(s => s.ExistsAsync(model.Name, model.CityId, null)).ReturnsAsync(false);

            var result = await _controller.Create(model) as RedirectToActionResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.ActionName, Is.EqualTo("Index"));
            _neighborhoodServiceMock.Verify(
                s => s.CreateAsync(It.Is<NeighborhoodCreateDTO>(dto => dto.Name == model.Name && dto.CityId == cityId && dto.SafetyRating == model.SafetyRating)),
                Times.Once);
        }

        [Test]
        public async Task Edit_Get_ShouldReturnNotFound_WhenIdIsNull()
        {
            var result = await _controller.Edit(null) as NotFoundResult;

            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task Edit_Get_ShouldReturnNotFound_WhenNeighborhoodDoesNotExist()
        {
            _neighborhoodServiceMock.Setup(s => s.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((NeighborhoodDTO)null!);

            var result = await _controller.Edit(Guid.NewGuid()) as NotFoundResult;

            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task Edit_Get_ShouldReturnViewWithModelAndCities_WhenNeighborhoodExists()
        {
            var cities = CreateCities();
            var neighborhoodId = Guid.NewGuid();
            _neighborhoodServiceMock.Setup(s => s.GetByIdAsync(neighborhoodId)).ReturnsAsync(new NeighborhoodDTO
            {
                Id = neighborhoodId,
                Name = "Center",
                SafetyRating = 8,
                CityId = cities[1].Id
            });
            _cityServiceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(cities);

            var result = await _controller.Edit(neighborhoodId) as ViewResult;

            Assert.That(result, Is.Not.Null);
            var model = result!.Model as NeighborhoodEditViewModel;
            Assert.That(model, Is.Not.Null);
            Assert.That(model!.Id, Is.EqualTo(neighborhoodId));
            Assert.That(model.CityId, Is.EqualTo(cities[1].Id));
            Assert.That(model.Cities.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task Edit_Post_ShouldReturnNotFound_WhenIdMismatchesModelId()
        {
            var result = await _controller.Edit(Guid.NewGuid(), new NeighborhoodEditViewModel { Id = Guid.NewGuid() }) as NotFoundResult;

            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task Edit_Post_ShouldReturnViewAndRepopulateCities_WhenModelStateIsInvalid()
        {
            var cities = CreateCities();
            var neighborhoodId = Guid.NewGuid();
            var model = new NeighborhoodEditViewModel { Id = neighborhoodId, Name = "Center", CityId = cities[0].Id, SafetyRating = 8 };
            _controller.ModelState.AddModelError("Name", "Invalid");
            _cityServiceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(cities);

            var result = await _controller.Edit(neighborhoodId, model) as ViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Model, Is.EqualTo(model));
            Assert.That(model.Cities.Count(), Is.EqualTo(2));
            _neighborhoodServiceMock.Verify(s => s.UpdateAsync(It.IsAny<NeighborhoodEditDTO>()), Times.Never);
        }

        [Test]
        public async Task Edit_Post_ShouldReturnViewWithError_WhenNeighborhoodAlreadyExists()
        {
            var cities = CreateCities();
            var neighborhoodId = Guid.NewGuid();
            var model = new NeighborhoodEditViewModel { Id = neighborhoodId, Name = "Center", CityId = cities[0].Id, SafetyRating = 8 };
            _neighborhoodServiceMock.Setup(s => s.ExistsAsync(model.Name, model.CityId, model.Id)).ReturnsAsync(true);
            _cityServiceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(cities);

            var result = await _controller.Edit(neighborhoodId, model) as ViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(_controller.ModelState["Name"]!.Errors[0].ErrorMessage, Is.EqualTo("A neighborhood with this name already exists in the selected city."));
            Assert.That(model.Cities.Count(), Is.EqualTo(2));
            _neighborhoodServiceMock.Verify(s => s.UpdateAsync(It.IsAny<NeighborhoodEditDTO>()), Times.Never);
        }

        [Test]
        public async Task Edit_Post_ShouldRedirectToIndex_WhenModelIsValidAndNeighborhoodIsUnique()
        {
            var cityId = Guid.NewGuid();
            var neighborhoodId = Guid.NewGuid();
            var model = new NeighborhoodEditViewModel { Id = neighborhoodId, Name = "Center", CityId = cityId, SafetyRating = 8 };
            _neighborhoodServiceMock.Setup(s => s.ExistsAsync(model.Name, model.CityId, model.Id)).ReturnsAsync(false);

            var result = await _controller.Edit(neighborhoodId, model) as RedirectToActionResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.ActionName, Is.EqualTo("Index"));
            _neighborhoodServiceMock.Verify(
                s => s.UpdateAsync(It.Is<NeighborhoodEditDTO>(dto => dto.Id == neighborhoodId && dto.Name == model.Name && dto.CityId == cityId && dto.SafetyRating == model.SafetyRating)),
                Times.Once);
        }

        [Test]
        public async Task Delete_ShouldReturnNotFound_WhenIdIsNull()
        {
            var result = await _controller.Delete(null) as NotFoundResult;

            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task Delete_ShouldReturnNotFound_WhenNeighborhoodDoesNotExist()
        {
            _neighborhoodServiceMock.Setup(s => s.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((NeighborhoodDTO)null!);

            var result = await _controller.Delete(Guid.NewGuid()) as NotFoundResult;

            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task Delete_ShouldReturnViewWithNeighborhood_WhenNeighborhoodExists()
        {
            var neighborhoodId = Guid.NewGuid();
            var neighborhood = new NeighborhoodDTO { Id = neighborhoodId, Name = "Center", SafetyRating = 8, CityName = "Sofia" };
            _neighborhoodServiceMock.Setup(s => s.GetByIdAsync(neighborhoodId)).ReturnsAsync(neighborhood);

            var result = await _controller.Delete(neighborhoodId) as ViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Model, Is.EqualTo(neighborhood));
        }

        [Test]
        public async Task DeleteConfirmed_ShouldRedirectToIndex()
        {
            var neighborhoodId = Guid.NewGuid();

            var result = await _controller.DeleteConfirmed(neighborhoodId) as RedirectToActionResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.ActionName, Is.EqualTo("Index"));
            _neighborhoodServiceMock.Verify(s => s.DeleteAsync(neighborhoodId), Times.Once);
        }
    }
}

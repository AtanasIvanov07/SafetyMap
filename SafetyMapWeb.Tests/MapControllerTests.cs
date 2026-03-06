using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SafetyMap.Core.Contracts;
using SafetyMap.Core.DTOs.City;
using SafetyMap.Core.DTOs.CrimeCategory;
using SafetyMap.Core.DTOs.CrimeStatistic;
using SafetyMapWeb.Controllers;
using SafetyMapWeb.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SafetyMapWeb.Tests.Controllers
{
    [TestFixture]
    public class MapControllerTests
    {
        private Mock<IMapService> _mapServiceMock;
        private MapController _controller;

        [SetUp]
        public void SetUp()
        {
            _mapServiceMock = new Mock<IMapService>();
            _controller = new MapController(_mapServiceMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _controller?.Dispose();
        }

        [Test]
        public void Index_ShouldReturnView()
        {
            var result = _controller.Index() as ViewResult;
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task GetPopulationData_ShouldReturnJsonWithPopulationData()
        {
            var cities = new List<CityDTO>
            {
                new CityDTO { Name = "City 1", Population = 1000 },
                new CityDTO { Name = "City 2", Population = 2000 }
            };
            _mapServiceMock.Setup(s => s.GetPopulationDataAsync()).ReturnsAsync(cities);

            var result = await _controller.GetPopulationData() as JsonResult;

            Assert.That(result, Is.Not.Null);
            var data = result!.Value as IEnumerable<RegionPopulationViewModel>;
            Assert.That(data, Is.Not.Null);
            Assert.That(data!.Count(), Is.EqualTo(2));
            Assert.That(data!.Any(d => d.Name == "City 1"), Is.True);
        }

        [Test]
        public async Task GetCrimeData_ShouldReturnJsonWithCrimeData()
        {
            var crimeData = new List<CityCrimeDto>
            {
                new CityCrimeDto { CityName = "City 1", TotalCrimes = 10, CrimeRatePer1000 = 5 }
            };
            var categoryId = Guid.NewGuid();
            int year = 2023;

            _mapServiceMock.Setup(s => s.GetCrimeDataAsync(categoryId, year)).ReturnsAsync(crimeData);

            var result = await _controller.GetCrimeData(categoryId, year) as JsonResult;

            Assert.That(result, Is.Not.Null);
            var data = result!.Value as IEnumerable<CityCrimeDto>;
            Assert.That(data, Is.Not.Null);
            Assert.That(data!.Count(), Is.EqualTo(1));
            Assert.That(data!.First().CityName, Is.EqualTo("City 1"));
        }

        [Test]
        public async Task GetCrimeCategories_ShouldReturnJsonWithCategories()
        {
            var categories = new List<CrimeCategoryDTO>
            {
                new CrimeCategoryDTO { Id = Guid.NewGuid(), Name = "Category 1", ColorCode = "#000" }
            };
            _mapServiceMock.Setup(s => s.GetCrimeCategoriesAsync()).ReturnsAsync(categories);

            var result = await _controller.GetCrimeCategories() as JsonResult;

            Assert.That(result, Is.Not.Null);
            var data = result!.Value as IEnumerable<CrimeCategoryDTO>;
            Assert.That(data, Is.Not.Null);
            Assert.That(data!.Count(), Is.EqualTo(1));
            Assert.That(data!.First().Name, Is.EqualTo("Category 1"));
        }
    }
}

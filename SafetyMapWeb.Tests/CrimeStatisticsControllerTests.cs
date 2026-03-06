using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Moq;
using NUnit.Framework;
using SafetyMap.Core.Contracts;
using SafetyMap.Core.DTOs.CrimeStatistic;
using SafetyMapWeb.Controllers;
using SafetyMapWeb.Models.CrimeStatistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SafetyMapWeb.Tests.Controllers
{
    [TestFixture]
    public class CrimeStatisticsControllerTests
    {
        private Mock<ICrimeStatisticService> _crimeStatisticServiceMock;
        private CrimeStatisticsController _controller;

        [SetUp]
        public void SetUp()
        {
            _crimeStatisticServiceMock = new Mock<ICrimeStatisticService>();
            _controller = new CrimeStatisticsController(_crimeStatisticServiceMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _controller?.Dispose();
        }

        [Test]
        public async Task Index_ShouldReturnViewWithQueryModel()
        {
            var statistics = new List<CrimeStatisticDTO>
            {
                new CrimeStatisticDTO { Id = Guid.NewGuid(), NeighborhoodName = "N1", CrimeCategoryName = "C1", CountOfCrimes = 10, Year = 2023 }
            };
            
            _crimeStatisticServiceMock.Setup(s => s.GetAllAsync(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync((statistics, 1));

            var result = await _controller.Index(null, null, null, 1) as ViewResult;

            Assert.That(result, Is.Not.Null);
            var model = result!.Model as CrimeStatisticQueryViewModel;
            Assert.That(model, Is.Not.Null);
            Assert.That(model!.Statistics.Count, Is.EqualTo(1));
            Assert.That(model.TotalPages, Is.EqualTo(1));
        }

        [Test]
        public async Task Details_ShouldReturnNotFound_WhenIdIsNull()
        {
            var result = await _controller.Details(null) as NotFoundResult;
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task Details_ShouldReturnNotFound_WhenStatisticDoesNotExist()
        {
            _crimeStatisticServiceMock.Setup(s => s.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((CrimeStatisticDTO)null!);

            var result = await _controller.Details(Guid.NewGuid()) as NotFoundResult;
            
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task Details_ShouldReturnViewWithStatistic_WhenExists()
        {
            var statId = Guid.NewGuid();
            var stat = new CrimeStatisticDTO { Id = statId, NeighborhoodName = "N1", CrimeCategoryName = "C1", CountOfCrimes = 10, Year = 2023 };
            _crimeStatisticServiceMock.Setup(s => s.GetByIdAsync(statId)).ReturnsAsync(stat);

            var result = await _controller.Details(statId) as ViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Model, Is.EqualTo(stat));
        }

        [Test]
        public async Task Create_Get_ShouldReturnViewWithSelectLists()
        {
            var neighborhoods = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("1", "N1") };
            var categories = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("2", "C1") };
            
            _crimeStatisticServiceMock.Setup(s => s.GetNeighborhoodSelectListAsync()).ReturnsAsync(neighborhoods);
            _crimeStatisticServiceMock.Setup(s => s.GetCrimeCategorySelectListAsync()).ReturnsAsync(categories);

            var result = await _controller.Create() as ViewResult;

            Assert.That(result, Is.Not.Null);
            var model = result!.Model as CrimeStatisticCreateViewModel;
            Assert.That(model, Is.Not.Null);
            Assert.That(model!.Neighborhoods.Count, Is.EqualTo(1));
            Assert.That(model.CrimeCategories.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task Create_Post_ShouldReturnView_WhenModelStateIsInvalid()
        {
            _controller.ModelState.AddModelError("Error", "Invalid");
            var model = new CrimeStatisticCreateViewModel();
            
            _crimeStatisticServiceMock.Setup(s => s.GetNeighborhoodSelectListAsync()).ReturnsAsync(new List<KeyValuePair<string, string>>());
            _crimeStatisticServiceMock.Setup(s => s.GetCrimeCategorySelectListAsync()).ReturnsAsync(new List<KeyValuePair<string, string>>());

            var result = await _controller.Create(model) as ViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Model, Is.EqualTo(model));
        }

        [Test]
        public async Task Create_Post_ShouldRedirectToIndex_WhenValid()
        {
            var model = new CrimeStatisticCreateViewModel { NeighborhoodId = Guid.NewGuid(), CrimeCategoryId = Guid.NewGuid(), CountOfCrimes = 10, Year = 2023 };

            var result = await _controller.Create(model) as RedirectToActionResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.ActionName, Is.EqualTo("Index"));
            _crimeStatisticServiceMock.Verify(s => s.CreateAsync(It.IsAny<CrimeStatisticCreateDTO>()), Times.Once);
        }

        [Test]
        public async Task Edit_Get_ShouldReturnNotFound_WhenIdIsNull()
        {
            var result = await _controller.Edit(null) as NotFoundResult;
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task Edit_Get_ShouldReturnNotFound_WhenStatisticDoesNotExist()
        {
            _crimeStatisticServiceMock.Setup(s => s.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((CrimeStatisticDTO)null!);

            var result = await _controller.Edit(Guid.NewGuid()) as NotFoundResult;

            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task Edit_Get_ShouldReturnViewWithModelAndSelectLists_WhenExists()
        {
            var statId = Guid.NewGuid();
            var stat = new CrimeStatisticDTO { Id = statId, NeighborhoodId = Guid.NewGuid(), CrimeCategoryId = Guid.NewGuid(), CountOfCrimes = 10, Year = 2023 };
            
            _crimeStatisticServiceMock.Setup(s => s.GetByIdAsync(statId)).ReturnsAsync(stat);
            _crimeStatisticServiceMock.Setup(s => s.GetNeighborhoodSelectListAsync()).ReturnsAsync(new List<KeyValuePair<string, string>>());
            _crimeStatisticServiceMock.Setup(s => s.GetCrimeCategorySelectListAsync()).ReturnsAsync(new List<KeyValuePair<string, string>>());


            var result = await _controller.Edit(statId) as ViewResult;

            Assert.That(result, Is.Not.Null);
            var model = result!.Model as CrimeStatisticEditViewModel;
            Assert.That(model, Is.Not.Null);
            Assert.That(model!.Id, Is.EqualTo(statId));
            Assert.That(model.CountOfCrimes, Is.EqualTo(10));
        }

        [Test]
        public async Task Edit_Post_ShouldReturnNotFound_WhenIdMismatchesModelId()
        {
            var model = new CrimeStatisticEditViewModel { Id = Guid.NewGuid() };
            var result = await _controller.Edit(Guid.NewGuid(), model) as NotFoundResult;

            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task Edit_Post_ShouldReturnView_WhenModelStateIsInvalid()
        {
            var statId = Guid.NewGuid();
            var model = new CrimeStatisticEditViewModel { Id = statId };
            _controller.ModelState.AddModelError("Error", "Invalid");
            
            _crimeStatisticServiceMock.Setup(s => s.GetNeighborhoodSelectListAsync()).ReturnsAsync(new List<KeyValuePair<string, string>>());
            _crimeStatisticServiceMock.Setup(s => s.GetCrimeCategorySelectListAsync()).ReturnsAsync(new List<KeyValuePair<string, string>>());


            var result = await _controller.Edit(statId, model) as ViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Model, Is.EqualTo(model));
        }

        [Test]
        public async Task Edit_Post_ShouldRedirectToIndex_WhenValid()
        {
            var statId = Guid.NewGuid();
            var model = new CrimeStatisticEditViewModel { Id = statId, NeighborhoodId = Guid.NewGuid(), CrimeCategoryId = Guid.NewGuid(), CountOfCrimes = 15, Year = 2023 };

            var result = await _controller.Edit(statId, model) as RedirectToActionResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.ActionName, Is.EqualTo("Index"));
            _crimeStatisticServiceMock.Verify(s => s.UpdateAsync(It.IsAny<CrimeStatisticEditDTO>()), Times.Once);
        }

        [Test]
        public async Task Delete_Get_ShouldReturnNotFound_WhenIdIsNull()
        {
            var result = await _controller.Delete(null) as NotFoundResult;
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task Delete_Get_ShouldReturnNotFound_WhenStatisticDoesNotExist()
        {
            _crimeStatisticServiceMock.Setup(s => s.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((CrimeStatisticDTO)null!);

            var result = await _controller.Delete(Guid.NewGuid()) as NotFoundResult;

            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task Delete_Get_ShouldReturnViewWithStatistic_WhenExists()
        {
            var statId = Guid.NewGuid();
            var stat = new CrimeStatisticDTO { Id = statId, NeighborhoodName = "N", CrimeCategoryName = "C", CountOfCrimes = 10, Year = 2023 };
            _crimeStatisticServiceMock.Setup(s => s.GetByIdAsync(statId)).ReturnsAsync(stat);

            var result = await _controller.Delete(statId) as ViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Model, Is.EqualTo(stat));
        }

        [Test]
        public async Task DeleteConfirmed_ShouldRedirectToIndex()
        {
            var statId = Guid.NewGuid();

            var result = await _controller.DeleteConfirmed(statId) as RedirectToActionResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.ActionName, Is.EqualTo("Index"));
            _crimeStatisticServiceMock.Verify(s => s.DeleteAsync(statId), Times.Once);
        }
    }
}

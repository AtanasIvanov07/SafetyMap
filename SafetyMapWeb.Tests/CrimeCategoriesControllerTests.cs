using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SafetyMap.Core.Contracts;
using SafetyMap.Core.DTOs.CrimeCategory;
using SafetyMapWeb.Controllers;
using SafetyMapWeb.Models.CrimeCategories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SafetyMapWeb.Tests.Controllers
{
    [TestFixture]
    public class CrimeCategoriesControllerTests
    {
        private Mock<ICrimeCategoryService> _crimeCategoryServiceMock;
        private CrimeCategoriesController _controller;

        [SetUp]
        public void SetUp()
        {
            _crimeCategoryServiceMock = new Mock<ICrimeCategoryService>();
            _controller = new CrimeCategoriesController(_crimeCategoryServiceMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _controller?.Dispose();
        }

        [Test]
        public async Task Index_ShouldReturnViewWithCategories()
        {
            var categories = new List<CrimeCategoryDTO>
            {
                new CrimeCategoryDTO { Id = Guid.NewGuid(), Name = "Category 1", ColorCode = "#123" },
                new CrimeCategoryDTO { Id = Guid.NewGuid(), Name = "Category 2", ColorCode = "#456" }
            };
            _crimeCategoryServiceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(categories);

            var result = await _controller.Index() as ViewResult;

            Assert.That(result, Is.Not.Null);
            var model = result!.Model as List<CrimeCategoryIndexViewModel>;
            Assert.That(model, Is.Not.Null);
            Assert.That(model!.Count, Is.EqualTo(2));
            Assert.That(model.Any(m => m.Name == "Category 1"), Is.True);
        }

        [Test]
        public async Task Details_ShouldReturnNotFound_WhenIdIsNull()
        {
            var result = await _controller.Details(null) as NotFoundResult;
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task Details_ShouldReturnNotFound_WhenCategoryDoesNotExist()
        {
            _crimeCategoryServiceMock.Setup(s => s.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((CrimeCategoryDTO)null!);

            var result = await _controller.Details(Guid.NewGuid()) as NotFoundResult;
            
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task Details_ShouldReturnViewWithCategory_WhenCategoryExists()
        {
            var categoryId = Guid.NewGuid();
            var category = new CrimeCategoryDTO { Id = categoryId, Name = "Category", ColorCode = "#000" };
            _crimeCategoryServiceMock.Setup(s => s.GetByIdAsync(categoryId)).ReturnsAsync(category);

            var result = await _controller.Details(categoryId) as ViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Model, Is.EqualTo(category));
        }

        [Test]
        public async Task Create_Get_ShouldReturnView()
        {
            var result = await _controller.Create() as ViewResult;
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task Create_Post_ShouldReturnView_WhenModelStateIsInvalid()
        {
            _controller.ModelState.AddModelError("Error", "Invalid");
            var model = new CrimeCategoryCreateViewModel();

            var result = await _controller.Create(model) as ViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Model, Is.EqualTo(model));
        }

        [Test]
        public async Task Create_Post_ShouldRedirectToIndex_WhenValid()
        {
            var model = new CrimeCategoryCreateViewModel { Name = "New Category", ColorCode = "#FFF" };

            var result = await _controller.Create(model) as RedirectToActionResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.ActionName, Is.EqualTo("Index"));
            _crimeCategoryServiceMock.Verify(s => s.CreateAsync(It.IsAny<CrimeCategoryCreateDTO>()), Times.Once);
        }

        [Test]
        public async Task Edit_Get_ShouldReturnNotFound_WhenIdIsNull()
        {
            var result = await _controller.Edit(null) as NotFoundResult;
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task Edit_Get_ShouldReturnNotFound_WhenCategoryDoesNotExist()
        {
            _crimeCategoryServiceMock.Setup(s => s.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((CrimeCategoryDTO)null!);

            var result = await _controller.Edit(Guid.NewGuid()) as NotFoundResult;

            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task Edit_Get_ShouldReturnViewWithModel_WhenCategoryExists()
        {
            var categoryId = Guid.NewGuid();
            var category = new CrimeCategoryDTO { Id = categoryId, Name = "Category", ColorCode = "#000" };
            _crimeCategoryServiceMock.Setup(s => s.GetByIdAsync(categoryId)).ReturnsAsync(category);

            var result = await _controller.Edit(categoryId) as ViewResult;

            Assert.That(result, Is.Not.Null);
            var model = result!.Model as CrimeCategoryEditViewModel;
            Assert.That(model, Is.Not.Null);
            Assert.That(model!.Id, Is.EqualTo(categoryId));
            Assert.That(model.Name, Is.EqualTo("Category"));
        }

        [Test]
        public async Task Edit_Post_ShouldReturnNotFound_WhenIdMismatchesModelId()
        {
            var model = new CrimeCategoryEditViewModel { Id = Guid.NewGuid() };
            var result = await _controller.Edit(Guid.NewGuid(), model) as NotFoundResult;

            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task Edit_Post_ShouldReturnView_WhenModelStateIsInvalid()
        {
            var categoryId = Guid.NewGuid();
            var model = new CrimeCategoryEditViewModel { Id = categoryId };
            _controller.ModelState.AddModelError("Error", "Invalid");

            var result = await _controller.Edit(categoryId, model) as ViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Model, Is.EqualTo(model));
        }

        [Test]
        public async Task Edit_Post_ShouldRedirectToIndex_WhenValid()
        {
            var categoryId = Guid.NewGuid();
            var model = new CrimeCategoryEditViewModel { Id = categoryId, Name = "Updated", ColorCode = "#111" };

            var result = await _controller.Edit(categoryId, model) as RedirectToActionResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.ActionName, Is.EqualTo("Index"));
            _crimeCategoryServiceMock.Verify(s => s.UpdateAsync(It.IsAny<CrimeCategoryEditDTO>()), Times.Once);
        }

        [Test]
        public async Task Delete_Get_ShouldReturnNotFound_WhenIdIsNull()
        {
            var result = await _controller.Delete(null) as NotFoundResult;
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task Delete_Get_ShouldReturnNotFound_WhenCategoryDoesNotExist()
        {
            _crimeCategoryServiceMock.Setup(s => s.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((CrimeCategoryDTO)null!);

            var result = await _controller.Delete(Guid.NewGuid()) as NotFoundResult;

            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task Delete_Get_ShouldReturnViewWithCategory_WhenCategoryExists()
        {
            var categoryId = Guid.NewGuid();
            var category = new CrimeCategoryDTO { Id = categoryId, Name = "Category", ColorCode = "#000" };
            _crimeCategoryServiceMock.Setup(s => s.GetByIdAsync(categoryId)).ReturnsAsync(category);

            var result = await _controller.Delete(categoryId) as ViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Model, Is.EqualTo(category));
        }

        [Test]
        public async Task DeleteConfirmed_ShouldRedirectToIndex()
        {
            var categoryId = Guid.NewGuid();

            var result = await _controller.DeleteConfirmed(categoryId) as RedirectToActionResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.ActionName, Is.EqualTo("Index"));
            _crimeCategoryServiceMock.Verify(s => s.DeleteAsync(categoryId), Times.Once);
        }
    }
}

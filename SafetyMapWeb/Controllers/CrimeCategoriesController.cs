using Microsoft.AspNetCore.Mvc;
using SafetyMap.Core.Contracts;
using SafetyMap.Core.DTOs.CrimeCategory;
using SafetyMapWeb.Models.CrimeCategories;

namespace SafetyMapWeb.Controllers
{
    public class CrimeCategoriesController : Controller
    {
        private readonly ICrimeCategoryService _crimeCategoryService;

        public CrimeCategoriesController(ICrimeCategoryService crimeCategoryService)
        {
            _crimeCategoryService = crimeCategoryService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var categories = await _crimeCategoryService.GetAllAsync();

            var viewModels = categories.Select(c => new CrimeCategoryIndexViewModel
            {
                Id = c.Id,
                Name = c.Name,
                ColorCode = c.ColorCode
            }).ToList();

            return View(viewModels);
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var crimeCategory = await _crimeCategoryService.GetByIdAsync(id.Value);
            if (crimeCategory == null)
            {
                return NotFound();
            }

            return View(crimeCategory);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CrimeCategoryCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var dto = new CrimeCategoryCreateDTO
                {
                    Name = model.Name,
                    ColorCode = model.ColorCode
                };

                await _crimeCategoryService.CreateAsync(dto);
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var crimeCategory = await _crimeCategoryService.GetByIdAsync(id.Value);
            if (crimeCategory == null)
            {
                return NotFound();
            }

            var model = new CrimeCategoryEditViewModel
            {
                Id = crimeCategory.Id,
                Name = crimeCategory.Name,
                ColorCode = crimeCategory.ColorCode
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, CrimeCategoryEditViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var dto = new CrimeCategoryEditDTO
                {
                    Id = model.Id,
                    Name = model.Name,
                    ColorCode = model.ColorCode
                };

                await _crimeCategoryService.UpdateAsync(dto);
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var crimeCategory = await _crimeCategoryService.GetByIdAsync(id.Value);
            if (crimeCategory == null)
            {
                return NotFound();
            }

            return View(crimeCategory);
        }


        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            await _crimeCategoryService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

    }
}

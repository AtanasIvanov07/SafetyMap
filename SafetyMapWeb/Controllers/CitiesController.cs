using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SafetyMap.Core.Contracts;
using SafetyMap.Core.DTOs.City;
using SafetyMapWeb.Models.Cities;

namespace SafetyMapWeb.Controllers
{
    public class CitiesController : Controller
    {
        private readonly ICityService _cityService;

        public CitiesController(ICityService cityService)
        {
            _cityService = cityService;
        }

        [HttpGet]
        [Authorize(Roles = "User, Admin")]
        public async Task<IActionResult> Index()
        {
            var cities = await _cityService.GetAllAsync();

            var viewModels = cities.Select(c => new CityIndexViewModel
            {
                Id = c.Id,
                Name = c.Name,
                Population = c.Population
            }).ToList();

            return View(viewModels);
        }

        [HttpGet]
        [Authorize(Roles = "User, Admin")]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var city = await _cityService.GetByIdAsync(id.Value);
            if (city == null)
            {
                return NotFound();
            }

            return View(city);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CityCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var dto = new CityCreateDTO
                {
                    Name = model.Name,
                    Population = model.Population
                };

                await _cityService.CreateAsync(dto);
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var city = await _cityService.GetByIdAsync(id.Value);
            if (city == null)
            {
                return NotFound();
            }

            var model = new CityEditViewModel
            {
                Id = city.Id,
                Name = city.Name,
                Population = city.Population
            };

            return View(model);
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(Guid id, CityEditViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                var dto = new CityEditDTO
                {
                    Id = model.Id,
                    Name = model.Name,
                    Population = model.Population
                };

                await _cityService.UpdateAsync(dto);
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var city = await _cityService.GetByIdAsync(id.Value);
            if (city == null)
            {
                return NotFound();
            }

            return View(city);
        }


        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            await _cityService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }


    }
}

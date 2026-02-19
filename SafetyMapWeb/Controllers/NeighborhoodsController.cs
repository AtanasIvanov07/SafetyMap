using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using SafetyMap.Core.Contracts;
using SafetyMap.Core.DTOs.Neighborhood;
using SafetyMapWeb.Models.Neighborhoods;

namespace SafetyMapWeb.Controllers
{
    public class NeighborhoodsController : Controller
    {
        private readonly INeighborhoodService _neighborhoodService;
        private readonly ICityService _cityService;

        public NeighborhoodsController(INeighborhoodService neighborhoodService, ICityService cityService)
        {
            _neighborhoodService = neighborhoodService;
            _cityService = cityService;
        }

        [HttpGet]
        [Authorize(Roles = "User, Admin")]
        public async Task<IActionResult> Index()
        {
            var neighborhoods = await _neighborhoodService.GetAllAsync();

            var viewModels = neighborhoods.Select(n => new NeighborhoodIndexViewModel
            {
                Id = n.Id,
                Name = n.Name,
                SafetyRating = n.SafetyRating,
                CityName = n.CityName
            }).ToList();

            return View(viewModels);
        }

        [HttpGet]
        [Authorize(Roles = "User, Admin")]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            var neighborhood = await _neighborhoodService.GetByIdAsync(id.Value);
            if (neighborhood == null) return NotFound();

            return View(neighborhood);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            var model = new NeighborhoodCreateViewModel();
            var cities = await _cityService.GetAllAsync();
            model.Cities = cities.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList();
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(NeighborhoodCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var dto = new NeighborhoodCreateDTO
                {
                    Name = model.Name,
                    SafetyRating = model.SafetyRating,
                    Latitude = model.Latitude,
                    Longitude = model.Longitude,
                    CityId = model.CityId
                };

                await _neighborhoodService.CreateAsync(dto);
                return RedirectToAction(nameof(Index));
            }
            var cities = await _cityService.GetAllAsync();
            model.Cities = cities.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList();
            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var neighborhood = await _neighborhoodService.GetByIdAsync(id.Value);
            if (neighborhood == null) return NotFound();

            var cities = await _cityService.GetAllAsync();
            var model = new NeighborhoodEditViewModel
            {
                Id = neighborhood.Id,
                Name = neighborhood.Name,
                SafetyRating = neighborhood.SafetyRating,
                Latitude = neighborhood.Latitude,
                Longitude = neighborhood.Longitude,
                CityId = neighborhood.CityId,
                Cities = cities.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList()
            };
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(Guid id, NeighborhoodEditViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var dto = new NeighborhoodEditDTO
                {
                    Id = model.Id,
                    Name = model.Name,
                    SafetyRating = model.SafetyRating,
                    Latitude = model.Latitude,
                    Longitude = model.Longitude,
                    CityId = model.CityId
                };

                await _neighborhoodService.UpdateAsync(dto);
                return RedirectToAction(nameof(Index));
            }
            var cities = await _cityService.GetAllAsync();
            model.Cities = cities.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList();
            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null) return NotFound();

            var neighborhood = await _neighborhoodService.GetByIdAsync(id.Value);
            if (neighborhood == null) return NotFound();

            return View(neighborhood);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            await _neighborhoodService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}

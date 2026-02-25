using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using SafetyMap.Core.Contracts;
using SafetyMap.Core.DTOs.CrimeStatistic;
using SafetyMapWeb.Models.CrimeStatistics;

namespace SafetyMapWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CrimeStatisticsController : Controller
    {
        private readonly ICrimeStatisticService _crimeStatisticService;

        public CrimeStatisticsController(ICrimeStatisticService crimeStatisticService)
        {
            _crimeStatisticService = crimeStatisticService;
        }

        [HttpGet]
        [Authorize(Roles = "User, Admin")]
        public async Task<IActionResult> Index([FromQuery] string? searchTerm, [FromQuery] int? year, [FromQuery] int currentPage = 1)
        {
            int itemsPerPage = 20;
            var (statistics, totalCount) = await _crimeStatisticService.GetAllAsync(searchTerm, year, currentPage, itemsPerPage);

            var viewModels = statistics.Select(c => new CrimeStatisticIndexViewModel
            {
                Id = c.Id,
                NeighborhoodName = c.NeighborhoodName,
                CrimeCategoryName = c.CrimeCategoryName,
                CountOfCrimes = c.CountOfCrimes,
                Year = c.Year,
                TrendPercentage = c.TrendPercentage
            }).ToList();

            var queryModel = new CrimeStatisticQueryViewModel
            {
                SearchTerm = searchTerm,
                Year = year,
                CurrentPage = currentPage,
                TotalPages = (int)Math.Ceiling(totalCount / (double)itemsPerPage),
                Statistics = viewModels
            };

            return View(queryModel);
        }

        [HttpGet]
        [Authorize(Roles = "User, Admin")]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            var crimeStatistic = await _crimeStatisticService.GetByIdAsync(id.Value);
            if (crimeStatistic == null) return NotFound();

            return View(crimeStatistic);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            var model = new CrimeStatisticCreateViewModel();
            var neighborhoods = await _crimeStatisticService.GetNeighborhoodSelectListAsync();
            var categories = await _crimeStatisticService.GetCrimeCategorySelectListAsync();
            model.Neighborhoods = neighborhoods.Select(n => new SelectListItem { Value = n.Key, Text = n.Value }).ToList();
            model.CrimeCategories = categories.Select(c => new SelectListItem { Value = c.Key, Text = c.Value }).ToList();
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CrimeStatisticCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var dto = new CrimeStatisticCreateDTO
                {
                    NeighborhoodId = model.NeighborhoodId,
                    CrimeCategoryId = model.CrimeCategoryId,
                    CountOfCrimes = model.CountOfCrimes,
                    Year = model.Year,
                    TrendPercentage = model.TrendPercentage
                };

                await _crimeStatisticService.CreateAsync(dto);
                return RedirectToAction(nameof(Index));
            }
            var neighborhoods = await _crimeStatisticService.GetNeighborhoodSelectListAsync();
            var categories = await _crimeStatisticService.GetCrimeCategorySelectListAsync();
            model.Neighborhoods = neighborhoods.Select(n => new SelectListItem { Value = n.Key, Text = n.Value }).ToList();
            model.CrimeCategories = categories.Select(c => new SelectListItem { Value = c.Key, Text = c.Value }).ToList();
            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var crimeStatistic = await _crimeStatisticService.GetByIdAsync(id.Value);
            if (crimeStatistic == null) return NotFound();

            var neighborhoods = await _crimeStatisticService.GetNeighborhoodSelectListAsync();
            var categories = await _crimeStatisticService.GetCrimeCategorySelectListAsync();

            var model = new CrimeStatisticEditViewModel
            {
                Id = crimeStatistic.Id,
                NeighborhoodId = crimeStatistic.NeighborhoodId,
                CrimeCategoryId = crimeStatistic.CrimeCategoryId,
                CountOfCrimes = crimeStatistic.CountOfCrimes,
                Year = crimeStatistic.Year,
                TrendPercentage = crimeStatistic.TrendPercentage,
                Neighborhoods = neighborhoods.Select(n => new SelectListItem { Value = n.Key, Text = n.Value }).ToList(),
                CrimeCategories = categories.Select(c => new SelectListItem { Value = c.Key, Text = c.Value }).ToList()
            };
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(Guid id, CrimeStatisticEditViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var dto = new CrimeStatisticEditDTO
                {
                    Id = model.Id,
                    NeighborhoodId = model.NeighborhoodId,
                    CrimeCategoryId = model.CrimeCategoryId,
                    CountOfCrimes = model.CountOfCrimes,
                    Year = model.Year,
                    TrendPercentage = model.TrendPercentage
                };

                await _crimeStatisticService.UpdateAsync(dto);
                return RedirectToAction(nameof(Index));
            }
            var neighborhoods = await _crimeStatisticService.GetNeighborhoodSelectListAsync();
            var categories = await _crimeStatisticService.GetCrimeCategorySelectListAsync();
            model.Neighborhoods = neighborhoods.Select(n => new SelectListItem { Value = n.Key, Text = n.Value }).ToList();
            model.CrimeCategories = categories.Select(c => new SelectListItem { Value = c.Key, Text = c.Value }).ToList();
            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null) return NotFound();

            var crimeStatistic = await _crimeStatisticService.GetByIdAsync(id.Value);
            if (crimeStatistic == null) return NotFound();

            return View(crimeStatistic);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            await _crimeStatisticService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}

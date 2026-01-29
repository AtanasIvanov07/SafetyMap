using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SafetyMapData;
using SafetyMapData.Entities;
using SafetyMapWeb.Models.CrimeStatistics;

namespace SafetyMapWeb.Controllers
{
    public class CrimeStatisticsController : Controller
    {
        private readonly SafetyMapDbContext _context;

        public CrimeStatisticsController(SafetyMapDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var crimeStatistics = await _context.CrimeStatistics
                .Include(c => c.Neighborhood)
                .Include(c => c.CrimeCategory)
                .Select(c => new CrimeStatisticIndexViewModel
                {
                    Id = c.Id,
                    NeighborhoodName = c.Neighborhood != null ? c.Neighborhood.Name : "N/A",
                    CrimeCategoryName = c.CrimeCategory != null ? c.CrimeCategory.Name : "N/A",
                    CountOfCrimes = c.CountOfCrimes,
                    Year = c.Year,
                    TrendPercentage = c.TrendPercentage
                })
                .ToListAsync();
            return View(crimeStatistics);
        }

        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            var crimeStatistic = await _context.CrimeStatistics
                .Include(c => c.Neighborhood)
                .Include(c => c.CrimeCategory)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (crimeStatistic == null) return NotFound();

            return View(crimeStatistic);
        }

        public IActionResult Create()
        {
            var model = new CrimeStatisticCreateViewModel();
            model.Neighborhoods = _context.Neighborhoods.Select(n => new SelectListItem { Value = n.Id.ToString(), Text = n.Name }).ToList();
            model.CrimeCategories = _context.CrimeCategories.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CrimeStatisticCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var crimeStatistic = new CrimeStatistic
                {
                    Id = Guid.NewGuid(),
                    NeighborhoodId = model.NeighborhoodId,
                    CrimeCategoryId = model.CrimeCategoryId,
                    CountOfCrimes = model.CountOfCrimes,
                    Year = model.Year,
                    TrendPercentage = model.TrendPercentage
                };
                _context.Add(crimeStatistic);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            model.Neighborhoods = _context.Neighborhoods.Select(n => new SelectListItem { Value = n.Id.ToString(), Text = n.Name }).ToList();
            model.CrimeCategories = _context.CrimeCategories.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList();
            return View(model);
        }

        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var crimeStatistic = await _context.CrimeStatistics.FindAsync(id);
            if (crimeStatistic == null) return NotFound();

            var model = new CrimeStatisticEditViewModel
            {
                Id = crimeStatistic.Id,
                NeighborhoodId = crimeStatistic.NeighborhoodId,
                CrimeCategoryId = crimeStatistic.CrimeCategoryId,
                CountOfCrimes = crimeStatistic.CountOfCrimes,
                Year = crimeStatistic.Year,
                TrendPercentage = crimeStatistic.TrendPercentage,
                Neighborhoods = _context.Neighborhoods.Select(n => new SelectListItem { Value = n.Id.ToString(), Text = n.Name }).ToList(),
                CrimeCategories = _context.CrimeCategories.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, CrimeStatisticEditViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var crimeStatistic = await _context.CrimeStatistics.FindAsync(id);
                if (crimeStatistic == null) return NotFound();

                crimeStatistic.NeighborhoodId = model.NeighborhoodId;
                crimeStatistic.CrimeCategoryId = model.CrimeCategoryId;
                crimeStatistic.CountOfCrimes = model.CountOfCrimes;
                crimeStatistic.Year = model.Year;
                crimeStatistic.TrendPercentage = model.TrendPercentage;

                _context.Update(crimeStatistic);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            model.Neighborhoods = _context.Neighborhoods.Select(n => new SelectListItem { Value = n.Id.ToString(), Text = n.Name }).ToList();
            model.CrimeCategories = _context.CrimeCategories.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList();
            return View(model);
        }

        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null) return NotFound();

            var crimeStatistic = await _context.CrimeStatistics
                .Include(c => c.Neighborhood)
                .Include(c => c.CrimeCategory)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (crimeStatistic == null) return NotFound();

            return View(crimeStatistic);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var crimeStatistic = await _context.CrimeStatistics.FindAsync(id);
            if (crimeStatistic != null)
            {
                _context.CrimeStatistics.Remove(crimeStatistic);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}

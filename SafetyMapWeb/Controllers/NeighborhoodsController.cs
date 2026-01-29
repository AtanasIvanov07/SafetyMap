using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SafetyMapData;
using SafetyMapData.Entities;
using SafetyMapWeb.Models.Neighborhoods;

namespace SafetyMapWeb.Controllers
{
    public class NeighborhoodsController : Controller
    {
        private readonly SafetyMapDbContext _context;

        public NeighborhoodsController(SafetyMapDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var neighborhoods = await _context.Neighborhoods
                .Include(n => n.City)
                .Select(n => new NeighborhoodIndexViewModel
                {
                    Id = n.Id,
                    Name = n.Name,
                    SafetyRating = n.SafetyRating,
                    CityName = n.City != null ? n.City.Name : "N/A"
                })
                .ToListAsync();
            return View(neighborhoods);
        }

        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            var neighborhood = await _context.Neighborhoods
                .Include(n => n.City)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (neighborhood == null) return NotFound();

            return View(neighborhood);
        }

        public IActionResult Create()
        {
            var model = new NeighborhoodCreateViewModel();
            model.Cities = _context.Cities.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(NeighborhoodCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var neighborhood = new Neighborhood
                {
                    Id = Guid.NewGuid(),
                    Name = model.Name,
                    SafetyRating = model.SafetyRating,
                    Latitude = model.Latitude,
                    Longitude = model.Longitude,
                    CityId = model.CityId
                };
                _context.Add(neighborhood);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            model.Cities = _context.Cities.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList();
            return View(model);
        }

        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var neighborhood = await _context.Neighborhoods.FindAsync(id);
            if (neighborhood == null) return NotFound();

            var model = new NeighborhoodEditViewModel
            {
                Id = neighborhood.Id,
                Name = neighborhood.Name,
                SafetyRating = neighborhood.SafetyRating,
                Latitude = neighborhood.Latitude,
                Longitude = neighborhood.Longitude,
                CityId = neighborhood.CityId,
                Cities = _context.Cities.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, NeighborhoodEditViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var neighborhood = await _context.Neighborhoods.FindAsync(id);
                if (neighborhood == null) return NotFound();

                neighborhood.Name = model.Name;
                neighborhood.SafetyRating = model.SafetyRating;
                neighborhood.Latitude = model.Latitude;
                neighborhood.Longitude = model.Longitude;
                neighborhood.CityId = model.CityId;

                _context.Update(neighborhood);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            model.Cities = _context.Cities.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList();
            return View(model);
        }

        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null) return NotFound();

            var neighborhood = await _context.Neighborhoods
                .Include(n => n.City)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (neighborhood == null) return NotFound();

            return View(neighborhood);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var neighborhood = await _context.Neighborhoods.FindAsync(id);
            if (neighborhood != null)
            {
                _context.Neighborhoods.Remove(neighborhood);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}

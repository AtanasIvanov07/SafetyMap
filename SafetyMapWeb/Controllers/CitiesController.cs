using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafetyMapData;
using SafetyMapData.Entities;
using SafetyMapWeb.Models.Cities;

namespace SafetyMapWeb.Controllers
{
    public class CitiesController : Controller
    {
        private readonly SafetyMapDbContext _context;

        public CitiesController(SafetyMapDbContext context)
        {
            _context = context;

        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var cities = await _context.Cities
                .Select(c => new CityIndexViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    State = c.State,
                    ImageUrl = c.ImageUrl,
                    Population = c.Population
                })
                .ToListAsync();

            return View(cities);
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var city = await _context.Cities
                .FirstOrDefaultAsync(m => m.Id == id);
            if (city == null)
            {
                return NotFound();
            }

            return View(city);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CityCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var city = new City
                {
                    Id = Guid.NewGuid(),
                    Name = model.Name,
                    State = model.State,
                    ImageUrl = model.ImageUrl,
                    Population = model.Population
                };

                _context.Add(city);
                await _context.SaveChangesAsync();
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

            var city = await _context.Cities.FindAsync(id);
            if (city == null)
            {
                return NotFound();
            }

            var model = new CityEditViewModel
            {
                Id = city.Id,
                Name = city.Name,
                State = city.State,
                ImageUrl = city.ImageUrl,
                Population = city.Population
            };

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, CityEditViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {

                var city = await _context.Cities.FindAsync(id);
                if (city == null)
                {
                    return NotFound();
                }
                city.Name = model.Name;
                city.State = model.State;
                city.ImageUrl = model.ImageUrl;
                city.Population = model.Population;

                _context.Update(city);
                await _context.SaveChangesAsync();
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

            var city = await _context.Cities
                .FirstOrDefaultAsync(m => m.Id == id);
            if (city == null)
            {
                return NotFound();
            }

            return View(city);
        }


        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var city = await _context.Cities.FindAsync(id);
            if (city != null)
            {
                _context.Cities.Remove(city);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


    }
}

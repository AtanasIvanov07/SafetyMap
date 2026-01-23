using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafetyMapData;
using SafetyMapData.Entities;
using SafetyMapWeb.Models.CrimeCategories;

namespace SafetyMapWeb.Controllers
{
    public class CrimeCategoriesController : Controller
    {
        private readonly SafetyMapDbContext _context;

        public CrimeCategoriesController(SafetyMapDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var categories = await _context.CrimeCategories
                 .Select(c => new CrimeCategoryIndexViewModel
                 {
                     Id = c.Id,
                     Name = c.Name,
                     ColorCode = c.ColorCode
                 })
                 .ToListAsync();

            return View(categories);
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var crimeCategory = await _context.CrimeCategories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (crimeCategory == null)
            {
                return NotFound();
            }

            return View(crimeCategory);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CrimeCategoryCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var crimeCategory = new CrimeCategory
                {
                    Id = Guid.NewGuid(),
                    Name = model.Name,
                    ColorCode = model.ColorCode
                };
                _context.Add(crimeCategory);
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

            var crimeCategory = await _context.CrimeCategories.FindAsync(id);
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

        [HttpGet]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, CrimeCategoryEditViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var crimeCategory = await _context.CrimeCategories.FindAsync(id);
                if (crimeCategory == null)
                {
                    return NotFound();
                }
                crimeCategory.Name = model.Name;
                crimeCategory.ColorCode = model.ColorCode;
                _context.Update(crimeCategory);
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

            var crimeCategory = await _context.CrimeCategories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (crimeCategory == null)
            {
                return NotFound();
            }

            return View(crimeCategory);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var crimeCategory = await _context.CrimeCategories.FindAsync(id);
            if (crimeCategory != null)
            {
                _context.CrimeCategories.Remove(crimeCategory);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}

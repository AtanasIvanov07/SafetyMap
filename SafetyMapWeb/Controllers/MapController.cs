using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafetyMapData;
using SafetyMapWeb.Models.ViewModels;

namespace SafetyMapWeb.Controllers
{
    public class MapController : Controller
    {
        private readonly SafetyMapDbContext _context;

        public MapController(SafetyMapDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetPopulationData()
        {
            var data = await _context.Cities
                .Select(c => new RegionPopulationViewModel
                {
                    Name = c.Name,
                    Population = c.Population
                })
                .ToListAsync();

            return Json(data);
        }
    }
}

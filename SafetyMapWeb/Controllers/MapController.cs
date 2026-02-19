using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SafetyMap.Core.Contracts;
using SafetyMapWeb.Models.ViewModels;

namespace SafetyMapWeb.Controllers
{
    public class MapController : Controller
    {
        private readonly IMapService _mapService;

        public MapController(IMapService mapService)
        {
            _mapService = mapService;
        }

        [Authorize(Roles = "User, Admin")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "User, Admin")]
        public async Task<IActionResult> GetPopulationData()
        {
            var cities = await _mapService.GetPopulationDataAsync();

            var data = cities.Select(c => new RegionPopulationViewModel
            {
                Name = c.Name,
                Population = c.Population
            });

            return Json(data);
        }
    }
}

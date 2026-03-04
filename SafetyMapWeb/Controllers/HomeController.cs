using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SafetyMap.Core.Contracts;
using SafetyMapWeb.Models;

namespace SafetyMapWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ICityService _cityService;
        private readonly INeighborhoodService _neighborhoodService;
        private readonly ICrimeStatisticService _crimeStatisticService;
        private readonly IUserSubscriptionService _userSubscriptionService;

        public HomeController(
            ILogger<HomeController> logger,
            ICityService cityService,
            INeighborhoodService neighborhoodService,
            ICrimeStatisticService crimeStatisticService,
            IUserSubscriptionService userSubscriptionService)
        {
            _logger = logger;
            _cityService = cityService;
            _neighborhoodService = neighborhoodService;
            _crimeStatisticService = crimeStatisticService;
            _userSubscriptionService = userSubscriptionService;
        }

        public async Task<IActionResult> Index()
        {
            var cities = await _cityService.GetAllAsync();
            ViewBag.TotalCities = cities.Count();

            var neighborhoods = await _neighborhoodService.GetAllAsync();
            ViewBag.TotalNeighborhoods = neighborhoods.Count();

            var statsResult = await _crimeStatisticService.GetAllAsync(null, null, null, 1, 1);
            ViewBag.TotalCrimeStatistics = statsResult.TotalCount;

            var subscriptions = await _userSubscriptionService.GetAllAsync();
            ViewBag.TotalActiveAlerts = subscriptions.Count();

            return View();
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

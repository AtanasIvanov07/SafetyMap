using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafetyMap.Core.Contracts;
using System.Security.Claims;

namespace SafetyMapWeb.Controllers
{
    [Authorize]
    public class AnalyticsController : Controller
    {
        private readonly IAnalyticsService _analyticsService;

        public AnalyticsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            return View();
        }

        // ── JSON API Endpoints ──

        [HttpGet]
        public async Task<IActionResult> GetCrimeTrend()
        {
            var data = await _analyticsService.GetCrimeTrendOverTimeAsync();
            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetCategoryBreakdown(int? year)
        {
            var data = await _analyticsService.GetCategoryBreakdownAsync(year);
            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetNeighborhoodRankings(int? year, int top = 10)
        {
            var data = await _analyticsService.GetNeighborhoodRankingsAsync(year, top);
            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetCategoryTrends()
        {
            var data = await _analyticsService.GetCategoryTrendsOverTimeAsync();
            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetUserSubscribedTrends()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var data = await _analyticsService.GetUserSubscribedTrendsAsync(userId);
            return Json(data);
        }
    }
}

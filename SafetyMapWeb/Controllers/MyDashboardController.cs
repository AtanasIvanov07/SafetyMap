using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using SafetyMap.Core.Contracts;
using SafetyMap.Core.DTOs.UserSubscription;

namespace SafetyMapWeb.Controllers
{
    [Authorize]
    public class MyDashboardController : Controller
    {
        private readonly IUserSubscriptionService _userSubscriptionService;
        private readonly ICrimeStatisticService _crimeStatisticService;

        public MyDashboardController(IUserSubscriptionService userSubscriptionService, ICrimeStatisticService crimeStatisticService)
        {
            _userSubscriptionService = userSubscriptionService;
            _crimeStatisticService = crimeStatisticService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var subscriptions = await _userSubscriptionService.GetUserSubscriptionsAsync(userId);
            var statistics = await _crimeStatisticService.GetUserSubscribedStatisticsAsync(userId);
            var neighborhoodList = await _userSubscriptionService.GetNeighborhoodSelectListAsync();

            ViewBag.Subscriptions = subscriptions.ToList();
            ViewBag.Statistics = statistics.ToList();
            ViewBag.NeighborhoodList = neighborhoodList.ToList();
            ViewBag.SubscriptionCount = subscriptions.Count();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Subscribe(Guid neighborhoodId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            try
            {
                var dto = new UserSubscriptionCreateDTO
                {
                    UserId = userId,
                    NeighborhoodId = neighborhoodId
                };
                await _userSubscriptionService.CreateAsync(dto);
                TempData["SuccessMessage"] = "Successfully subscribed to the neighborhood alerts.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while attempting to subscribe.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Unsubscribe(Guid subscriptionId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var subscription = await _userSubscriptionService.GetByIdAsync(subscriptionId);
            if (subscription == null || subscription.UserId != userId)
            {
                return NotFound();
            }

            await _userSubscriptionService.DeleteAsync(subscriptionId);
            TempData["SuccessMessage"] = "Successfully unsubscribed.";

            return RedirectToAction(nameof(Index));
        }
    }
}

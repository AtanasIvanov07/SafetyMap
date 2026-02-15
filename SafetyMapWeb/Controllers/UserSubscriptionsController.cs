using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SafetyMap.Core.Contracts;
using SafetyMap.Core.DTOs.UserSubscription;
using SafetyMapWeb.Models.UserSubscriptions;

namespace SafetyMapWeb.Controllers
{
    public class UserSubscriptionsController : Controller
    {
        private readonly IUserSubscriptionService _userSubscriptionService;

        public UserSubscriptionsController(IUserSubscriptionService userSubscriptionService)
        {
            _userSubscriptionService = userSubscriptionService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var subscriptions = await _userSubscriptionService.GetAllAsync();

            var viewModels = subscriptions.Select(u => new UserSubscriptionIndexViewModel
            {
                Id = u.Id,
                UserId = u.UserId,
                NeighborhoodName = u.NeighborhoodName,
                SubscribedAt = u.SubscribedAt
            }).ToList();

            return View(viewModels);
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            var subscription = await _userSubscriptionService.GetByIdAsync(id.Value);
            if (subscription == null) return NotFound();

            return View(subscription);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new UserSubscriptionCreateViewModel();
            var neighborhoods = await _userSubscriptionService.GetNeighborhoodSelectListAsync();
            model.Neighborhoods = neighborhoods.Select(n => new SelectListItem { Value = n.Key, Text = n.Value }).ToList();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(UserSubscriptionCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var dto = new UserSubscriptionCreateDTO
                {
                    UserId = model.UserId,
                    NeighborhoodId = model.NeighborhoodId
                };

                await _userSubscriptionService.CreateAsync(dto);
                return RedirectToAction(nameof(Index));
            }
            var neighborhoods = await _userSubscriptionService.GetNeighborhoodSelectListAsync();
            model.Neighborhoods = neighborhoods.Select(n => new SelectListItem { Value = n.Key, Text = n.Value }).ToList();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var subscription = await _userSubscriptionService.GetByIdAsync(id.Value);
            if (subscription == null) return NotFound();

            var neighborhoods = await _userSubscriptionService.GetNeighborhoodSelectListAsync();
            var model = new UserSubscriptionEditViewModel
            {
                Id = subscription.Id,
                UserId = subscription.UserId,
                NeighborhoodId = subscription.NeighborhoodId,
                SubscribedAt = subscription.SubscribedAt,
                Neighborhoods = neighborhoods.Select(n => new SelectListItem { Value = n.Key, Text = n.Value }).ToList()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, UserSubscriptionEditViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var dto = new UserSubscriptionEditDTO
                {
                    Id = model.Id,
                    UserId = model.UserId,
                    NeighborhoodId = model.NeighborhoodId,
                    SubscribedAt = model.SubscribedAt
                };

                await _userSubscriptionService.UpdateAsync(dto);
                return RedirectToAction(nameof(Index));
            }
            var neighborhoods = await _userSubscriptionService.GetNeighborhoodSelectListAsync();
            model.Neighborhoods = neighborhoods.Select(n => new SelectListItem { Value = n.Key, Text = n.Value }).ToList();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null) return NotFound();

            var subscription = await _userSubscriptionService.GetByIdAsync(id.Value);
            if (subscription == null) return NotFound();

            return View(subscription);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            await _userSubscriptionService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}

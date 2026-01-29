using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SafetyMapData;
using SafetyMapData.Entities;
using SafetyMapWeb.Models.UserSubscriptions;

namespace SafetyMapWeb.Controllers
{
    public class UserSubscriptionsController : Controller
    {
        private readonly SafetyMapDbContext _context;

        public UserSubscriptionsController(SafetyMapDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var subscriptions = await _context.UserSubscriptions
                .Include(u => u.Neighborhood)
                .Select(u => new UserSubscriptionIndexViewModel
                {
                    Id = u.Id,
                    UserId = u.UserId,
                    NeighborhoodName = u.Neighborhood != null ? u.Neighborhood.Name : "N/A",
                    SubscribedAt = u.SubscribedAt
                })
                .ToListAsync();
            return View(subscriptions);
        }
        [HttpGet]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            var subscription = await _context.UserSubscriptions
                .Include(u => u.Neighborhood)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (subscription == null) return NotFound();

            return View(subscription);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new UserSubscriptionCreateViewModel();
            model.Neighborhoods = _context.Neighborhoods.Select(n => new SelectListItem { Value = n.Id.ToString(), Text = n.Name }).ToList();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(UserSubscriptionCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var subscription = new UserSubscription
                {
                    Id = Guid.NewGuid(),
                    UserId = model.UserId,
                    NeighborhoodId = model.NeighborhoodId,
                    SubscribedAt = DateTime.Now
                };
                _context.Add(subscription);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            model.Neighborhoods = _context.Neighborhoods.Select(n => new SelectListItem { Value = n.Id.ToString(), Text = n.Name }).ToList();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var subscription = await _context.UserSubscriptions.FindAsync(id);
            if (subscription == null) return NotFound();

            var model = new UserSubscriptionEditViewModel
            {
                Id = subscription.Id,
                UserId = subscription.UserId,
                NeighborhoodId = subscription.NeighborhoodId,
                SubscribedAt = subscription.SubscribedAt,
                Neighborhoods = _context.Neighborhoods.Select(n => new SelectListItem { Value = n.Id.ToString(), Text = n.Name }).ToList()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, UserSubscriptionEditViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var subscription = await _context.UserSubscriptions.FindAsync(id);
                if (subscription == null) return NotFound();

                subscription.UserId = model.UserId;
                subscription.NeighborhoodId = model.NeighborhoodId;
                subscription.SubscribedAt = model.SubscribedAt;

                _context.Update(subscription);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            model.Neighborhoods = _context.Neighborhoods.Select(n => new SelectListItem { Value = n.Id.ToString(), Text = n.Name }).ToList();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null) return NotFound();

            var subscription = await _context.UserSubscriptions
                .Include(u => u.Neighborhood)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (subscription == null) return NotFound();

            return View(subscription);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var subscription = await _context.UserSubscriptions.FindAsync(id);
            if (subscription != null)
            {
                _context.UserSubscriptions.Remove(subscription);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}

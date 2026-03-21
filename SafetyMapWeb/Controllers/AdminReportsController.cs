using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafetyMap.Core.Contracts;

namespace SafetyMapWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminReportsController : Controller
    {
        private readonly IUserCrimeReportService _userCrimeReportService;

        public AdminReportsController(IUserCrimeReportService userCrimeReportService)
        {
            _userCrimeReportService = userCrimeReportService;
        }

        public async Task<IActionResult> Index()
        {
            var pendingReports = await _userCrimeReportService.GetPendingReportsAsync();
            return View(pendingReports);
        }

        [HttpPost]
        public async Task<IActionResult> Approve(Guid id)
        {
            try
            {
                await _userCrimeReportService.ApproveReportAsync(id);
                TempData["SuccessMessage"] = "Report approved and statistic updated successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to approve report: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Reject(Guid id)
        {
            try
            {
                await _userCrimeReportService.RejectReportAsync(id);
                TempData["SuccessMessage"] = "Report rejected successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to reject report: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

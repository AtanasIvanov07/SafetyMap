using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SafetyMap.Core.Contracts;
using SafetyMap.Core.DTOs;
using SafetyMap.Core.DTOs.Neighborhood;
using SafetyMap.Core.DTOs.UserCrimeReport;
using SafetyMapWeb.Models.UserCrimeReports;

namespace SafetyMapWeb.Controllers
{
    [Authorize]
    public class UserCrimeReportsController : Controller
    {
        private readonly IUserCrimeReportService _userCrimeReportService;
        private readonly ICityService _cityService;
        private readonly ICrimeCategoryService _crimeCategoryService;
        private readonly INeighborhoodService _neighborhoodService;
        private readonly IPhotoService _photoService;

        public UserCrimeReportsController(
            IUserCrimeReportService userCrimeReportService,
            ICityService cityService,
            ICrimeCategoryService crimeCategoryService,
            INeighborhoodService neighborhoodService,
            IPhotoService photoService)
        {
            _userCrimeReportService = userCrimeReportService;
            _cityService = cityService;
            _crimeCategoryService = crimeCategoryService;
            _neighborhoodService = neighborhoodService;
            _photoService = photoService;
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new UserCrimeReportViewModel();
            await PopulateDropdowns(model);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(UserCrimeReportViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(model);
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            var dto = new UserCrimeReportCreateDTO
            {
                Description = model.Description,
                DateOfIncident = model.DateOfIncident,
                CrimeCategoryId = model.CrimeCategoryId,
                CityId = model.CityId,
                NeighborhoodId = model.NeighborhoodId
            };

            if (model.ImageFiles != null && model.ImageFiles.Count > 0)
            {
                var distinctSignatures = new HashSet<string>();
                var filesToProcess = model.ImageFiles.Take(5).ToList();

                foreach (var file in filesToProcess)
                {
                    if (file.Length > 0)
                    {
                        var signature = $"{file.FileName}_{file.Length}";
                        if (distinctSignatures.Add(signature))
                        {
                            var imageUrl = await _photoService.AddPhotoAsync(file);
                            if (imageUrl != null)
                            {
                                dto.ImageUrls.Add(imageUrl);
                            }
                        }
                    }
                }
            }

            await _userCrimeReportService.SubmitReportAsync(dto, userId);

            TempData["SuccessMessage"] = "Thank you for your report! It is currently pending review by an administrator.";
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> MyReports()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            var reports = await _userCrimeReportService.GetReportsByUserAsync(userId);
            return View(reports);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteImage(Guid imageId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            var result = await _userCrimeReportService.DeleteImageAsync(imageId, userId);

            if (result)
            {
                TempData["SuccessMessage"] = "Image removed successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Could not remove the image. It may not exist or you don't have permission.";
            }

            return RedirectToAction("MyReports");
        }

        private async Task PopulateDropdowns(UserCrimeReportViewModel model)
        {
            var cities = await _cityService.GetAllAsync();
            model.Cities = cities.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            });

            var categories = await _crimeCategoryService.GetAllAsync();
            model.CrimeCategories = categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            });

            var neighborhoods = await _neighborhoodService.GetAllAsync();
            if (model.CityId != Guid.Empty)
            {
                neighborhoods = neighborhoods.Where(n => n.CityId == model.CityId);
            }
            else
            {
                neighborhoods = new List<NeighborhoodDTO>();
            }

            model.Neighborhoods = neighborhoods.Select(n => new SelectListItem
            {
                Value = n.Id.ToString(),
                Text = n.Name
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetNeighborhoodsByCity(Guid cityId)
        {
            var neighborhoods = await _neighborhoodService.GetAllAsync();
            var filtered = neighborhoods.Where(n => n.CityId == cityId)
                .Select(n => new { id = n.Id, name = n.Name })
                .ToList();

            return Json(filtered);
        }
    }
}

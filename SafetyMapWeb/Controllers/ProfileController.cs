using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SafetyMap.Core.Contracts;
using SafetyMapData.Entities;
using SafetyMapWeb.Models;

namespace SafetyMapWeb.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<UserIdentity> _userManager;
        private readonly SignInManager<UserIdentity> _signInManager;
        private readonly IPhotoService _photoService;

        public ProfileController(
            UserManager<UserIdentity> userManager,
            SignInManager<UserIdentity> signInManager,
            IPhotoService photoService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _photoService = photoService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var model = new ProfileViewModel
            {
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                ProfilePictureUrl = user.ProfilePictureUrl
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                model.ProfilePictureUrl = user.ProfilePictureUrl; // Re-populate for view
                return View(model);
            }

            // Update basic details
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            
            if (user.Email != model.Email)
            {
                var emailExists = await _userManager.FindByEmailAsync(model.Email);
                if (emailExists != null && emailExists.Id != user.Id)
                {
                    ModelState.AddModelError("Email", "Email is already taken.");
                    model.ProfilePictureUrl = user.ProfilePictureUrl;
                    return View(model);
                }

                user.Email = model.Email;
                user.UserName = model.Email;
            }

            // Update password if provided
            if (!string.IsNullOrEmpty(model.NewPassword) && !string.IsNullOrEmpty(model.CurrentPassword))
            {
                var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                if (!changePasswordResult.Succeeded)
                {
                    foreach (var error in changePasswordResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    model.ProfilePictureUrl = user.ProfilePictureUrl;
                    return View(model);
                }
            }
            else if (!string.IsNullOrEmpty(model.NewPassword) && string.IsNullOrEmpty(model.CurrentPassword))
            {
                ModelState.AddModelError("CurrentPassword", "Current password is required to set a new password.");
                model.ProfilePictureUrl = user.ProfilePictureUrl;
                return View(model);
            }

            // Upload new profile picture if provided
            if (model.ProfilePicture != null)
            {
                if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
                {
                    await _photoService.DeletePhotoAsync(user.ProfilePictureUrl);
                }

                var newPhotoUrl = await _photoService.AddPhotoAsync(model.ProfilePicture);
                if (newPhotoUrl != null)
                {
                    user.ProfilePictureUrl = newPhotoUrl;
                }
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (updateResult.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["SuccessMessage"] = "Profile updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in updateResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            model.ProfilePictureUrl = user.ProfilePictureUrl;
            return View(model);
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SafetyMap.Core.Contracts;
using SafetyMapData.Entities;
using SafetyMapWeb.Models;
using SafetyMapWeb.Models.Account;

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
                ProfilePictureUrl = user.ProfilePictureUrl,
                Is2faEnabled = await _userManager.GetTwoFactorEnabledAsync(user)
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
                model.Is2faEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
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
                    model.Is2faEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
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
                    model.Is2faEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
                    return View(model);
                }
            }
            else if (!string.IsNullOrEmpty(model.NewPassword) && string.IsNullOrEmpty(model.CurrentPassword))
            {
                ModelState.AddModelError("CurrentPassword", "Current password is required to set a new password.");
                model.ProfilePictureUrl = user.ProfilePictureUrl;
                model.Is2faEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
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
            model.Is2faEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> EnableAuthenticator()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            if (string.IsNullOrEmpty(unformattedKey))
            {
                await _userManager.ResetAuthenticatorKeyAsync(user);
                unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            }

            var email = await _userManager.GetEmailAsync(user);
            var authenticatorUri = $"otpauth://totp/SafetyMap:{email}?secret={unformattedKey}&issuer=SafetyMap&digits=6";

            var model = new EnableAuthenticatorViewModel
            {
                SharedKey = unformattedKey ?? string.Empty,
                AuthenticatorUri = authenticatorUri
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EnableAuthenticator(EnableAuthenticatorViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                model.SharedKey = await _userManager.GetAuthenticatorKeyAsync(user) ?? string.Empty;
                var email = await _userManager.GetEmailAsync(user);
                model.AuthenticatorUri = $"otpauth://totp/SafetyMap:{email}?secret={model.SharedKey}&issuer=SafetyMap&digits=6";
                return View(model);
            }


            var verificationCode = model.Code.Replace(" ", string.Empty).Replace("-", string.Empty);

            var is2faTokenValid = await _userManager.VerifyTwoFactorTokenAsync(
                user, _userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

            if (!is2faTokenValid)
            {
                ModelState.AddModelError("Code", "Verification code is invalid.");
                model.SharedKey = await _userManager.GetAuthenticatorKeyAsync(user) ?? string.Empty;
                var email = await _userManager.GetEmailAsync(user);
                model.AuthenticatorUri = $"otpauth://totp/SafetyMap:{email}?secret={model.SharedKey}&issuer=SafetyMap&digits=6";
                return View(model);
            }

            await _userManager.SetTwoFactorEnabledAsync(user, true);
            await _signInManager.RefreshSignInAsync(user);

            TempData["SuccessMessage"] = "Your authenticator app has been verified. Two-factor authentication is now enabled.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Disable2fa()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var disable2faResult = await _userManager.SetTwoFactorEnabledAsync(user, false);
            if (!disable2faResult.Succeeded)
            {
                throw new InvalidOperationException($"Unexpected error occurred disabling 2FA for user with ID '{_userManager.GetUserId(User)}'.");
            }

            await _signInManager.RefreshSignInAsync(user);
            TempData["SuccessMessage"] = "Two-factor authentication has been disabled.";
            return RedirectToAction(nameof(Index));
        }
    }
}

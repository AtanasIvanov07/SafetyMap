using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using SafetyMap.Core.Contracts;
using SafetyMapData.Entities;
using System.Security.Claims;

namespace SafetyMap.Core.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<UserIdentity> _userManager;
        private readonly SignInManager<UserIdentity> _signInManager;

        public AccountService(
            UserManager<UserIdentity> userManager,
            SignInManager<UserIdentity> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IdentityResult> RegisterAsync(string userName, string email, string firstName, string lastName, string password)
        {
            var user = new UserIdentity
            {
                UserName = userName,
                Email = email,
                FirstName = firstName,
                LastName = lastName
            };

            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");
                await _signInManager.SignInAsync(user, isPersistent: false);
            }

            return result;
        }

        public async Task<bool> LoginAsync(string userName, string password)
        {
            var result = await _signInManager.PasswordSignInAsync(userName, password, false, false);
            return result.Succeeded;
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public AuthenticationProperties GetExternalLoginProperties(string provider, string? redirectUrl)
        {
            return _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        }

        public async Task<SignInResult> ExternalLoginCallbackAsync()
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return SignInResult.Failed;
            }

            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                return result;
            }

            if (!result.Succeeded)
            {
                var email = info.Principal.FindFirstValue(ClaimTypes.Email) ?? $"{info.ProviderKey}@{info.LoginProvider.ToLower()}.com";
                var firstName = info.Principal.FindFirstValue(ClaimTypes.GivenName) ?? info.Principal.FindFirstValue(ClaimTypes.Name) ?? "External";
                var lastName = info.Principal.FindFirstValue(ClaimTypes.Surname) ?? "User";

                if (email != null)
                {
                    var user = await _userManager.FindByEmailAsync(email);

                    if (user == null)
                    {
                        user = new UserIdentity
                        {
                            UserName = email,
                            Email = email,
                            FirstName = firstName,
                            LastName = lastName
                        };
                        var createResult = await _userManager.CreateAsync(user);
                        if (!createResult.Succeeded) return SignInResult.Failed;

                        await _userManager.AddToRoleAsync(user, "User");
                    }

                    var addLoginResult = await _userManager.AddLoginAsync(user, info);
                    if (addLoginResult.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return SignInResult.Success;
                    }
                }
            }

            return SignInResult.Failed;
        }
    }
}

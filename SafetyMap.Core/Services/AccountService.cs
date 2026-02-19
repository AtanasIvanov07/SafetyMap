using Microsoft.AspNetCore.Identity;
using SafetyMap.Core.Contracts;
using SafetyMapData.Entities;

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
    }
}

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace SafetyMap.Core.Contracts
{
    public interface IAccountService
    {
        Task<IdentityResult> RegisterAsync(string userName, string email, string firstName, string lastName, string password);
        Task<bool> LoginAsync(string userName, string password);
        Task LogoutAsync();
        AuthenticationProperties GetExternalLoginProperties(string provider, string? redirectUrl);
        Task<SignInResult> ExternalLoginCallbackAsync();
    }
}

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace SafetyMap.Core.Contracts
{
    public interface IAccountService
    {
        Task<IdentityResult> RegisterAsync(string userName, string email, string firstName, string lastName, string password);
        Task<SignInResult> LoginAsync(string userName, string password);
        Task LogoutAsync();
        AuthenticationProperties GetExternalLoginProperties(string provider, string? redirectUrl);
        Task<SignInResult> ExternalLoginCallbackAsync();
        Task<string?> GeneratePasswordResetTokenAsync(string email);
        Task<IdentityResult> ResetPasswordAsync(string email, string token, string newPassword);
        Task<SignInResult> TwoFactorAuthenticatorSignInAsync(string code, bool isPersistent, bool rememberClient);
    }
}

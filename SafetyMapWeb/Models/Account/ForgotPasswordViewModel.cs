using System.ComponentModel.DataAnnotations;

namespace SafetyMapWeb.Models.Account
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}

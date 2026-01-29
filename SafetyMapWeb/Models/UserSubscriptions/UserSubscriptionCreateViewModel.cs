using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SafetyMapWeb.Models.UserSubscriptions
{
    public class UserSubscriptionCreateViewModel
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public Guid NeighborhoodId { get; set; }

        public IEnumerable<SelectListItem> Neighborhoods { get; set; } = new List<SelectListItem>();
    }
}

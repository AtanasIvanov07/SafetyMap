using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SafetyMapWeb.Models.UserSubscriptions
{
    public class UserSubscriptionEditViewModel
    {
        public Guid Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public Guid NeighborhoodId { get; set; }

        [Required]
        public DateTime SubscribedAt { get; set; }

        public IEnumerable<SelectListItem> Neighborhoods { get; set; } = new List<SelectListItem>();
    }
}

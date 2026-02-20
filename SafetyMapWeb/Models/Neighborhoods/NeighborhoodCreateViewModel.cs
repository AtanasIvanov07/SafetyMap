using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SafetyMapWeb.Models.Neighborhoods
{
    public class NeighborhoodCreateViewModel
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Range(1, 10)]
        public int SafetyRating { get; set; }

        [Required]
        public Guid CityId { get; set; }

        public IEnumerable<SelectListItem> Cities { get; set; } = new List<SelectListItem>();
    }
}

using System.ComponentModel.DataAnnotations;

namespace SafetyMapWeb.Models.Cities
{
    public class CityCreateViewModel
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string State { get; set; } = string.Empty;

        public string ImageUrl { get; set; } = string.Empty;
    }
}

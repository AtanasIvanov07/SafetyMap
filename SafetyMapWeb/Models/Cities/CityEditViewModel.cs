using System.ComponentModel.DataAnnotations;

namespace SafetyMapWeb.Models.Cities
{
    public class CityEditViewModel
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string State { get; set; } = string.Empty;

        public string ImageUrl { get; set; } = string.Empty;
    }
}

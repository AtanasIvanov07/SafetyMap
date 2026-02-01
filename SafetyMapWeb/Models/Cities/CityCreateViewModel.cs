using System.ComponentModel.DataAnnotations;

namespace SafetyMapWeb.Models.Cities
{
    public class CityCreateViewModel
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;


        public int Population { get; set; }
    }
}

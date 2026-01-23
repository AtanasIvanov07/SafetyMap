using System.ComponentModel.DataAnnotations;

namespace SafetyMapWeb.Models.CrimeCategories
{
    public class CrimeCategoryCreateViewModel
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string ColorCode { get; set; } = string.Empty;
    }
}

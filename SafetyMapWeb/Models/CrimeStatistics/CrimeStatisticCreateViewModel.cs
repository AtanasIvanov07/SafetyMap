using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SafetyMapWeb.Models.CrimeStatistics
{
    public class CrimeStatisticCreateViewModel
    {
        [Required]
        public Guid NeighborhoodId { get; set; }

        [Required]
        public Guid CrimeCategoryId { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int CountOfCrimes { get; set; }

        [Required]
        [Range(2000, 2100)]
        public int Year { get; set; }

        public double TrendPercentage { get; set; }

        public IEnumerable<SelectListItem> Neighborhoods { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> CrimeCategories { get; set; } = new List<SelectListItem>();
    }
}

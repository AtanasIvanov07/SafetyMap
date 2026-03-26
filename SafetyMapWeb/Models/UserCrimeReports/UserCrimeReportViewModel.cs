using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SafetyMapWeb.Models.UserCrimeReports
{
    public class UserCrimeReportViewModel
    {
        [Required(ErrorMessage = "Please select a city")]
        [Display(Name = "City")]
        public Guid CityId { get; set; }

        [Required(ErrorMessage = "Please select a crime category")]
        [Display(Name = "Crime Category")]
        public Guid CrimeCategoryId { get; set; }

        [Display(Name = "Neighborhood (Optional)")]
        public Guid? NeighborhoodId { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(1500, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 1500 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date of incident is required")]
        [Display(Name = "Date of Incident")]
        [DataType(DataType.Date)]
        public DateTime DateOfIncident { get; set; } = DateTime.Now;

        [Display(Name = "Attach Photos (Optional)")]
        public List<IFormFile>? ImageFiles { get; set; }

        public IEnumerable<SelectListItem> Cities { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> CrimeCategories { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Neighborhoods { get; set; } = new List<SelectListItem>();
    }
}

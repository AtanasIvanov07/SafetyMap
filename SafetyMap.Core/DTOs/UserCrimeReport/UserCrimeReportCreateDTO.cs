using System;
using System.ComponentModel.DataAnnotations;

namespace SafetyMap.Core.DTOs.UserCrimeReport
{
    public class UserCrimeReportCreateDTO
    {
        [Required]
        [StringLength(1500, MinimumLength = 10)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime DateOfIncident { get; set; }

        [Required]
        public Guid CrimeCategoryId { get; set; }

        [Required]
        public Guid CityId { get; set; }

        public Guid? NeighborhoodId { get; set; }

        public string? ImageUrl { get; set; }
    }
}

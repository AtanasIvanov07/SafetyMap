using System;

namespace SafetyMap.Core.DTOs
{
    public class PendingReportDTO
    {
        public Guid Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime DateOfIncident { get; set; }
        public string CrimeCategoryName { get; set; } = string.Empty;
        public string CityName { get; set; } = string.Empty;
        public string? NeighborhoodName { get; set; }
        public string ReporterName { get; set; } = string.Empty;
        public string ReporterEmail { get; set; } = string.Empty;
    }
}

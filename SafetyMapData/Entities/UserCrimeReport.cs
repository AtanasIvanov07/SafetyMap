using System;
using System.Collections.Generic;
using SafetyMapData.Enums;

namespace SafetyMapData.Entities
{
    public class UserCrimeReport
    {
        public Guid Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime DateOfIncident { get; set; }
        public ReportStatus Status { get; set; } = ReportStatus.Pending;

        public Guid CrimeCategoryId { get; set; }
        public CrimeCategory CrimeCategory { get; set; } = null!;

        public Guid CityId { get; set; }
        public City City { get; set; } = null!;

        public Guid? NeighborhoodId { get; set; }
        public Neighborhood? Neighborhood { get; set; }

        public string UserId { get; set; } = string.Empty;
        public UserIdentity UserIdentity { get; set; } = null!;

        public ICollection<UserCrimeReportImage> Images { get; set; } = new List<UserCrimeReportImage>();
    }
}

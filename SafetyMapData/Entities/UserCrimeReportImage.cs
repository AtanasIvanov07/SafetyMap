using System;

namespace SafetyMapData.Entities
{
    public class UserCrimeReportImage
    {
        public Guid Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;

        public Guid UserCrimeReportId { get; set; }
        public UserCrimeReport UserCrimeReport { get; set; } = null!;
    }
}

using System;

namespace SafetyMap.Core.DTOs.UserCrimeReport
{
    public class ReportImageDTO
    {
        public Guid Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
    }
}

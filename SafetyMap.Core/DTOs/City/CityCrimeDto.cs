using System;
using System.Collections.Generic;

namespace SafetyMap.Core.DTOs.City
{
    public class CityCrimeDto
    {
        public Guid CityId { get; set; }
        public string CityName { get; set; } = string.Empty;
        public int TotalCrimes { get; set; }
        public Dictionary<string, int> CrimesByCategory { get; set; } = new Dictionary<string, int>();
    }
}

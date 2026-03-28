namespace SafetyMap.Core.DTOs.Analytics
{
    public class NeighborhoodRankingDTO
    {
        public string NeighborhoodName { get; set; } = string.Empty;
        public string CityName { get; set; } = string.Empty;
        public int TotalCrimes { get; set; }
        public int SafetyRating { get; set; }
    }
}

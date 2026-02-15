namespace SafetyMap.Core.DTOs.CrimeStatistic
{
    public class CrimeStatisticDTO
    {
        public Guid Id { get; set; }
        public Guid NeighborhoodId { get; set; }
        public string NeighborhoodName { get; set; } = string.Empty;
        public Guid CrimeCategoryId { get; set; }
        public string CrimeCategoryName { get; set; } = string.Empty;
        public int CountOfCrimes { get; set; }
        public int Year { get; set; }
        public double TrendPercentage { get; set; }
    }
}

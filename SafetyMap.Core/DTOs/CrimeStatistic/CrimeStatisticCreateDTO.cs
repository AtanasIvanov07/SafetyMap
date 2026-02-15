namespace SafetyMap.Core.DTOs.CrimeStatistic
{
    public class CrimeStatisticCreateDTO
    {
        public Guid NeighborhoodId { get; set; }
        public Guid CrimeCategoryId { get; set; }
        public int CountOfCrimes { get; set; }
        public int Year { get; set; }
        public double TrendPercentage { get; set; }
    }
}

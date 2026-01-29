namespace SafetyMapWeb.Models.CrimeStatistics
{
    public class CrimeStatisticIndexViewModel
    {
        public Guid Id { get; set; }
        public string NeighborhoodName { get; set; } = string.Empty;
        public string CrimeCategoryName { get; set; } = string.Empty;
        public int CountOfCrimes { get; set; }
        public int Year { get; set; }
        public double TrendPercentage { get; set; }
    }
}

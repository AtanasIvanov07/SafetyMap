using SafetyMap.Core.DTOs.CrimeStatistic;

namespace SafetyMapWeb.Models.CrimeStatistics
{
    public class CrimeStatisticQueryViewModel
    {
        public string? SearchTerm { get; set; }
        public int? Year { get; set; }

        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }

        public IEnumerable<CrimeStatisticIndexViewModel> Statistics { get; set; } = new List<CrimeStatisticIndexViewModel>();
    }
}

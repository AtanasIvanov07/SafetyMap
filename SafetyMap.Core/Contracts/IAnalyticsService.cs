using SafetyMap.Core.DTOs.Analytics;

namespace SafetyMap.Core.Contracts
{
    public interface IAnalyticsService
    {
        /// <summary>
        /// Total crimes per year (aggregated across all categories/neighborhoods).
        /// </summary>
        Task<IEnumerable<CrimeTrendPointDTO>> GetCrimeTrendOverTimeAsync();

        /// <summary>
        /// Total crimes grouped by crime category (all years combined).
        /// </summary>
        Task<IEnumerable<CategoryBreakdownDTO>> GetCategoryBreakdownAsync(int? year = null);

        /// <summary>
        /// Neighborhoods ranked by total crime count (descending = most dangerous first).
        /// </summary>
        Task<IEnumerable<NeighborhoodRankingDTO>> GetNeighborhoodRankingsAsync(int? year = null, int top = 10);

        /// <summary>
        /// Per-category crime trends over years (for a multi-line chart).
        /// </summary>
        Task<IEnumerable<YearlyCategoryTrendDTO>> GetCategoryTrendsOverTimeAsync();

        /// <summary>
        /// Crime trends for neighborhoods the user is subscribed to.
        /// </summary>
        Task<IEnumerable<YearlyCategoryTrendDTO>> GetUserSubscribedTrendsAsync(string userId);
    }
}

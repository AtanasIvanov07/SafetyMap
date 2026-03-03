using SafetyMap.Core.DTOs.CrimeStatistic;

namespace SafetyMap.Core.Contracts
{
    public interface ICrimeStatisticService
    {
        Task<(IEnumerable<CrimeStatisticDTO> Statistics, int TotalCount)> GetAllAsync(string? neighborhoodSearch = null, string? categorySearch = null, int? year = null, int currentPage = 1, int itemsPerPage = 20);
        Task<IEnumerable<CrimeStatisticDTO>> GetUserSubscribedStatisticsAsync(string userId);
        Task<CrimeStatisticDTO?> GetByIdAsync(Guid id);
        Task CreateAsync(CrimeStatisticCreateDTO dto);
        Task UpdateAsync(CrimeStatisticEditDTO dto);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<KeyValuePair<string, string>>> GetNeighborhoodSelectListAsync();
        Task<IEnumerable<KeyValuePair<string, string>>> GetCrimeCategorySelectListAsync();
    }
}

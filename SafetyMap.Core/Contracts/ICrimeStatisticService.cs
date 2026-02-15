using SafetyMap.Core.DTOs.CrimeStatistic;

namespace SafetyMap.Core.Contracts
{
    public interface ICrimeStatisticService
    {
        Task<IEnumerable<CrimeStatisticDTO>> GetAllAsync();
        Task<CrimeStatisticDTO?> GetByIdAsync(Guid id);
        Task CreateAsync(CrimeStatisticCreateDTO dto);
        Task UpdateAsync(CrimeStatisticEditDTO dto);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<KeyValuePair<string, string>>> GetNeighborhoodSelectListAsync();
        Task<IEnumerable<KeyValuePair<string, string>>> GetCrimeCategorySelectListAsync();
    }
}

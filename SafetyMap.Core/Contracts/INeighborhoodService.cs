using SafetyMap.Core.DTOs.Neighborhood;

namespace SafetyMap.Core.Contracts
{
    public interface INeighborhoodService
    {
        Task<IEnumerable<NeighborhoodDTO>> GetAllAsync();
        Task<NeighborhoodDTO?> GetByIdAsync(Guid id);
        Task<bool> ExistsAsync(string name, Guid cityId, Guid? excludeId = null);
        Task CreateAsync(NeighborhoodCreateDTO dto);
        Task UpdateAsync(NeighborhoodEditDTO dto);
        Task DeleteAsync(Guid id);
    }
}

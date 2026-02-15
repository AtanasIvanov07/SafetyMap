using SafetyMap.Core.DTOs.City;

namespace SafetyMap.Core.Contracts
{
    public interface ICityService
    {
        Task<IEnumerable<CityDTO>> GetAllAsync();
        Task<CityDTO?> GetByIdAsync(Guid id);
        Task CreateAsync(CityCreateDTO dto);
        Task UpdateAsync(CityEditDTO dto);
        Task DeleteAsync(Guid id);
    }
}

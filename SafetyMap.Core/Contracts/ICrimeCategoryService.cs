using SafetyMap.Core.DTOs.CrimeCategory;

namespace SafetyMap.Core.Contracts
{
    public interface ICrimeCategoryService
    {
        Task<IEnumerable<CrimeCategoryDTO>> GetAllAsync();
        Task<CrimeCategoryDTO?> GetByIdAsync(Guid id);
        Task CreateAsync(CrimeCategoryCreateDTO dto);
        Task UpdateAsync(CrimeCategoryEditDTO dto);
        Task DeleteAsync(Guid id);
    }
}

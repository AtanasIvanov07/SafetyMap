using SafetyMap.Core.DTOs.City;
using SafetyMap.Core.DTOs.CrimeCategory;

namespace SafetyMap.Core.Contracts
{
    public interface IMapService
    {
        Task<IEnumerable<CityDTO>> GetPopulationDataAsync();
        Task<IEnumerable<CityCrimeDto>> GetCrimeDataAsync(Guid? categoryId);
        Task<IEnumerable<CrimeCategoryDTO>> GetCrimeCategoriesAsync();
    }
}

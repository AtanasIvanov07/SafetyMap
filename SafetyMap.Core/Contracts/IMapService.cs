using SafetyMap.Core.DTOs.City;
using SafetyMap.Core.DTOs.CrimeCategory;

namespace SafetyMap.Core.Contracts
{
    public interface IMapService
    {
        Task<IEnumerable<CityDTO>> GetPopulationDataAsync();
        Task<IEnumerable<CityCrimeDto>> GetCrimeDataAsync(Guid? categoryId, int? year = null);
        Task<IEnumerable<CrimeCategoryDTO>> GetCrimeCategoriesAsync();
    }
}

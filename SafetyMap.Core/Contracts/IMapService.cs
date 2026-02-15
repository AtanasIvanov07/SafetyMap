using SafetyMap.Core.DTOs.City;

namespace SafetyMap.Core.Contracts
{
    public interface IMapService
    {
        Task<IEnumerable<CityDTO>> GetPopulationDataAsync();
    }
}

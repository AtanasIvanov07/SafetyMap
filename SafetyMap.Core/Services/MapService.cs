using Microsoft.EntityFrameworkCore;
using SafetyMap.Core.Contracts;
using SafetyMap.Core.DTOs.City;
using SafetyMapData;

namespace SafetyMap.Core.Services
{
    public class MapService : IMapService
    {
        private readonly SafetyMapDbContext _context;

        public MapService(SafetyMapDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CityDTO>> GetPopulationDataAsync()
        {
            return await _context.Cities
                .Select(c => new CityDTO
                {
                    Id = c.Id,
                    Name = c.Name,
                    Population = c.Population
                })
                .ToListAsync();
        }
    }
}

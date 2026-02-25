using Microsoft.EntityFrameworkCore;
using SafetyMap.Core.Contracts;
using SafetyMap.Core.DTOs.City;
using SafetyMap.Core.DTOs.CrimeCategory;
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

        public async Task<IEnumerable<CityCrimeDto>> GetCrimeDataAsync(Guid? categoryId, int? year = null)
        {
            var cities = await _context.Cities
                .Include(c => c.Neighborhoods)
                    .ThenInclude(n => n.CrimeStatistics)
                        .ThenInclude(cs => cs.CrimeCategory)
                .ToListAsync();

            var result = new List<CityCrimeDto>();

            foreach (var city in cities)
            {
                var cityStats = city.Neighborhoods
                    .SelectMany(n => n.CrimeStatistics)
                    .Where(cs => (!categoryId.HasValue || cs.CrimeCategoryId == categoryId.Value) &&
                                 (!year.HasValue || cs.Year == year.Value))
                    .ToList();

                var dto = new CityCrimeDto
                {
                    CityId = city.Id,
                    CityName = city.Name,
                    TotalCrimes = cityStats.Sum(cs => cs.CountOfCrimes),
                    CrimesByCategory = cityStats
                        .GroupBy(cs => cs.CrimeCategory.Name)
                        .ToDictionary(g => g.Key, g => g.Sum(cs => cs.CountOfCrimes))
                };

                result.Add(dto);
            }

            return result;
        }

        public async Task<IEnumerable<CrimeCategoryDTO>> GetCrimeCategoriesAsync()
        {
            return await _context.CrimeCategories
                .Select(cc => new CrimeCategoryDTO
                {
                    Id = cc.Id,
                    Name = cc.Name,
                    ColorCode = cc.ColorCode
                })
                .ToListAsync();
        }
    }
}

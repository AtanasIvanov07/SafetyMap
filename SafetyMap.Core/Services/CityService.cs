using Microsoft.EntityFrameworkCore;
using SafetyMap.Core.Contracts;
using SafetyMap.Core.DTOs.City;
using SafetyMapData;
using SafetyMapData.Entities;

namespace SafetyMap.Core.Services
{
    public class CityService : ICityService
    {
        private readonly SafetyMapDbContext _context;

        public CityService(SafetyMapDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CityDTO>> GetAllAsync()
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

        public async Task<CityDTO?> GetByIdAsync(Guid id)
        {
            return await _context.Cities
                .Where(c => c.Id == id)
                .Select(c => new CityDTO
                {
                    Id = c.Id,
                    Name = c.Name,
                    Population = c.Population
                })
                .FirstOrDefaultAsync();
        }

        public async Task CreateAsync(CityCreateDTO dto)
        {
            var city = new City
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Population = dto.Population
            };

            _context.Cities.Add(city);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(CityEditDTO dto)
        {
            var city = await _context.Cities.FindAsync(dto.Id);

            if (city == null)
            {
                throw new ArgumentException($"City with Id '{dto.Id}' was not found.");
            }

            city.Name = dto.Name;
            city.Population = dto.Population;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var city = await _context.Cities.FindAsync(id);

            if (city != null)
            {
                _context.Cities.Remove(city);
                await _context.SaveChangesAsync();
            }
        }
    }
}

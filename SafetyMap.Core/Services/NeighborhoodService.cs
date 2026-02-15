using Microsoft.EntityFrameworkCore;
using SafetyMap.Core.Contracts;
using SafetyMap.Core.DTOs.Neighborhood;
using SafetyMapData;
using SafetyMapData.Entities;

namespace SafetyMap.Core.Services
{
    public class NeighborhoodService : INeighborhoodService
    {
        private readonly SafetyMapDbContext _context;

        public NeighborhoodService(SafetyMapDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<NeighborhoodDTO>> GetAllAsync()
        {
            return await _context.Neighborhoods
                .Include(n => n.City)
                .Select(n => new NeighborhoodDTO
                {
                    Id = n.Id,
                    Name = n.Name,
                    SafetyRating = n.SafetyRating,
                    Latitude = n.Latitude,
                    Longitude = n.Longitude,
                    CityId = n.CityId,
                    CityName = n.City != null ? n.City.Name : "N/A"
                })
                .ToListAsync();
        }

        public async Task<NeighborhoodDTO?> GetByIdAsync(Guid id)
        {
            return await _context.Neighborhoods
                .Include(n => n.City)
                .Where(n => n.Id == id)
                .Select(n => new NeighborhoodDTO
                {
                    Id = n.Id,
                    Name = n.Name,
                    SafetyRating = n.SafetyRating,
                    Latitude = n.Latitude,
                    Longitude = n.Longitude,
                    CityId = n.CityId,
                    CityName = n.City != null ? n.City.Name : "N/A"
                })
                .FirstOrDefaultAsync();
        }

        public async Task CreateAsync(NeighborhoodCreateDTO dto)
        {
            var neighborhood = new Neighborhood
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                SafetyRating = dto.SafetyRating,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                CityId = dto.CityId
            };

            _context.Neighborhoods.Add(neighborhood);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(NeighborhoodEditDTO dto)
        {
            var neighborhood = await _context.Neighborhoods.FindAsync(dto.Id);

            if (neighborhood == null)
            {
                throw new ArgumentException($"Neighborhood with Id '{dto.Id}' was not found.");
            }

            neighborhood.Name = dto.Name;
            neighborhood.SafetyRating = dto.SafetyRating;
            neighborhood.Latitude = dto.Latitude;
            neighborhood.Longitude = dto.Longitude;
            neighborhood.CityId = dto.CityId;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var neighborhood = await _context.Neighborhoods.FindAsync(id);

            if (neighborhood != null)
            {
                _context.Neighborhoods.Remove(neighborhood);
                await _context.SaveChangesAsync();
            }
        }
    }
}

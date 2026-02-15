using Microsoft.EntityFrameworkCore;
using SafetyMap.Core.Contracts;
using SafetyMap.Core.DTOs.CrimeStatistic;
using SafetyMapData;
using SafetyMapData.Entities;

namespace SafetyMap.Core.Services
{
    public class CrimeStatisticService : ICrimeStatisticService
    {
        private readonly SafetyMapDbContext _context;

        public CrimeStatisticService(SafetyMapDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CrimeStatisticDTO>> GetAllAsync()
        {
            return await _context.CrimeStatistics
                .Include(c => c.Neighborhood)
                .Include(c => c.CrimeCategory)
                .Select(c => new CrimeStatisticDTO
                {
                    Id = c.Id,
                    NeighborhoodId = c.NeighborhoodId,
                    NeighborhoodName = c.Neighborhood != null ? c.Neighborhood.Name : "N/A",
                    CrimeCategoryId = c.CrimeCategoryId,
                    CrimeCategoryName = c.CrimeCategory != null ? c.CrimeCategory.Name : "N/A",
                    CountOfCrimes = c.CountOfCrimes,
                    Year = c.Year,
                    TrendPercentage = c.TrendPercentage
                })
                .ToListAsync();
        }

        public async Task<CrimeStatisticDTO?> GetByIdAsync(Guid id)
        {
            return await _context.CrimeStatistics
                .Include(c => c.Neighborhood)
                .Include(c => c.CrimeCategory)
                .Where(c => c.Id == id)
                .Select(c => new CrimeStatisticDTO
                {
                    Id = c.Id,
                    NeighborhoodId = c.NeighborhoodId,
                    NeighborhoodName = c.Neighborhood != null ? c.Neighborhood.Name : "N/A",
                    CrimeCategoryId = c.CrimeCategoryId,
                    CrimeCategoryName = c.CrimeCategory != null ? c.CrimeCategory.Name : "N/A",
                    CountOfCrimes = c.CountOfCrimes,
                    Year = c.Year,
                    TrendPercentage = c.TrendPercentage
                })
                .FirstOrDefaultAsync();
        }

        public async Task CreateAsync(CrimeStatisticCreateDTO dto)
        {
            var crimeStatistic = new CrimeStatistic
            {
                Id = Guid.NewGuid(),
                NeighborhoodId = dto.NeighborhoodId,
                CrimeCategoryId = dto.CrimeCategoryId,
                CountOfCrimes = dto.CountOfCrimes,
                Year = dto.Year,
                TrendPercentage = dto.TrendPercentage
            };

            _context.CrimeStatistics.Add(crimeStatistic);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(CrimeStatisticEditDTO dto)
        {
            var crimeStatistic = await _context.CrimeStatistics.FindAsync(dto.Id);

            if (crimeStatistic == null)
            {
                throw new ArgumentException($"CrimeStatistic with Id '{dto.Id}' was not found.");
            }

            crimeStatistic.NeighborhoodId = dto.NeighborhoodId;
            crimeStatistic.CrimeCategoryId = dto.CrimeCategoryId;
            crimeStatistic.CountOfCrimes = dto.CountOfCrimes;
            crimeStatistic.Year = dto.Year;
            crimeStatistic.TrendPercentage = dto.TrendPercentage;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var crimeStatistic = await _context.CrimeStatistics.FindAsync(id);

            if (crimeStatistic != null)
            {
                _context.CrimeStatistics.Remove(crimeStatistic);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<KeyValuePair<string, string>>> GetNeighborhoodSelectListAsync()
        {
            return await _context.Neighborhoods
                .Select(n => new KeyValuePair<string, string>(n.Id.ToString(), n.Name))
                .ToListAsync();
        }

        public async Task<IEnumerable<KeyValuePair<string, string>>> GetCrimeCategorySelectListAsync()
        {
            return await _context.CrimeCategories
                .Select(c => new KeyValuePair<string, string>(c.Id.ToString(), c.Name))
                .ToListAsync();
        }
    }
}

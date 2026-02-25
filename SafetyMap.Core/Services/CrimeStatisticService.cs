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

        public async Task<(IEnumerable<CrimeStatisticDTO> Statistics, int TotalCount)> GetAllAsync(string? searchTerm = null, int? year = null, int currentPage = 1, int itemsPerPage = 20)
        {
            var query = _context.CrimeStatistics
                .Include(c => c.Neighborhood)
                .Include(c => c.CrimeCategory)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lowerSearchTerm = searchTerm.ToLower();
                query = query.Where(c => (c.Neighborhood != null && c.Neighborhood.Name.ToLower().Contains(lowerSearchTerm)) ||
                                         (c.CrimeCategory != null && c.CrimeCategory.Name.ToLower().Contains(lowerSearchTerm)));
            }

            if (year.HasValue)
            {
                query = query.Where(c => c.Year == year.Value);
            }

            int totalCount = await query.CountAsync();

            var statistics = await query
                .OrderByDescending(c => c.Year)
                .ThenBy(c => c.Neighborhood.Name)
                .Skip((currentPage - 1) * itemsPerPage)
                .Take(itemsPerPage)
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

            return (statistics, totalCount);
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

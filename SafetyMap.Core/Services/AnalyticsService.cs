using Microsoft.EntityFrameworkCore;
using SafetyMap.Core.Contracts;
using SafetyMap.Core.DTOs.Analytics;
using SafetyMapData;

namespace SafetyMap.Core.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly SafetyMapDbContext _context;

        public AnalyticsService(SafetyMapDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CrimeTrendPointDTO>> GetCrimeTrendOverTimeAsync()
        {
            return await _context.CrimeStatistics
                .GroupBy(cs => cs.Year)
                .Select(g => new CrimeTrendPointDTO
                {
                    Year = g.Key,
                    TotalCrimes = g.Sum(cs => cs.CountOfCrimes)
                })
                .OrderBy(t => t.Year)
                .ToListAsync();
        }

        public async Task<IEnumerable<CategoryBreakdownDTO>> GetCategoryBreakdownAsync(int? year = null)
        {
            var query = _context.CrimeStatistics
                .Include(cs => cs.CrimeCategory)
                .AsQueryable();

            if (year.HasValue)
            {
                query = query.Where(cs => cs.Year == year.Value);
            }

            return await query
                .GroupBy(cs => new { cs.CrimeCategory.Name, cs.CrimeCategory.ColorCode })
                .Select(g => new CategoryBreakdownDTO
                {
                    CategoryName = g.Key.Name,
                    ColorCode = g.Key.ColorCode,
                    TotalCrimes = g.Sum(cs => cs.CountOfCrimes)
                })
                .OrderByDescending(c => c.TotalCrimes)
                .ToListAsync();
        }

        public async Task<IEnumerable<NeighborhoodRankingDTO>> GetNeighborhoodRankingsAsync(int? year = null, int top = 10)
        {
            var query = _context.CrimeStatistics
                .Include(cs => cs.Neighborhood)
                    .ThenInclude(n => n.City)
                .AsQueryable();

            if (year.HasValue)
            {
                query = query.Where(cs => cs.Year == year.Value);
            }

            return await query
                .GroupBy(cs => new
                {
                    cs.Neighborhood.Name,
                    CityName = cs.Neighborhood.City != null ? cs.Neighborhood.City.Name : "N/A",
                    cs.Neighborhood.SafetyRating
                })
                .Select(g => new NeighborhoodRankingDTO
                {
                    NeighborhoodName = g.Key.Name,
                    CityName = g.Key.CityName,
                    TotalCrimes = g.Sum(cs => cs.CountOfCrimes),
                    SafetyRating = g.Key.SafetyRating
                })
                .OrderByDescending(n => n.TotalCrimes)
                .Take(top)
                .ToListAsync();
        }

        public async Task<IEnumerable<YearlyCategoryTrendDTO>> GetCategoryTrendsOverTimeAsync()
        {
            var data = await _context.CrimeStatistics
                .Include(cs => cs.CrimeCategory)
                .GroupBy(cs => new { cs.CrimeCategory.Name, cs.CrimeCategory.ColorCode, cs.Year })
                .Select(g => new
                {
                    CategoryName = g.Key.Name,
                    ColorCode = g.Key.ColorCode,
                    Year = g.Key.Year,
                    TotalCrimes = g.Sum(cs => cs.CountOfCrimes)
                })
                .OrderBy(x => x.Year)
                .ToListAsync();

            return data
                .GroupBy(d => new { d.CategoryName, d.ColorCode })
                .Select(g => new YearlyCategoryTrendDTO
                {
                    CategoryName = g.Key.CategoryName,
                    ColorCode = g.Key.ColorCode,
                    DataPoints = g.Select(x => new CrimeTrendPointDTO
                    {
                        Year = x.Year,
                        TotalCrimes = x.TotalCrimes
                    }).OrderBy(p => p.Year).ToList()
                })
                .ToList();
        }

        public async Task<IEnumerable<YearlyCategoryTrendDTO>> GetUserSubscribedTrendsAsync(string userId)
        {
            var subscribedNeighborhoodIds = await _context.UserSubscriptions
                .Where(us => us.UserId == userId)
                .Select(us => us.NeighborhoodId)
                .ToListAsync();

            if (!subscribedNeighborhoodIds.Any())
            {
                return Enumerable.Empty<YearlyCategoryTrendDTO>();
            }

            var data = await _context.CrimeStatistics
                .Include(cs => cs.CrimeCategory)
                .Where(cs => subscribedNeighborhoodIds.Contains(cs.NeighborhoodId))
                .GroupBy(cs => new { cs.CrimeCategory.Name, cs.CrimeCategory.ColorCode, cs.Year })
                .Select(g => new
                {
                    CategoryName = g.Key.Name,
                    ColorCode = g.Key.ColorCode,
                    Year = g.Key.Year,
                    TotalCrimes = g.Sum(cs => cs.CountOfCrimes)
                })
                .OrderBy(x => x.Year)
                .ToListAsync();

            return data
                .GroupBy(d => new { d.CategoryName, d.ColorCode })
                .Select(g => new YearlyCategoryTrendDTO
                {
                    CategoryName = g.Key.CategoryName,
                    ColorCode = g.Key.ColorCode,
                    DataPoints = g.Select(x => new CrimeTrendPointDTO
                    {
                        Year = x.Year,
                        TotalCrimes = x.TotalCrimes
                    }).OrderBy(p => p.Year).ToList()
                })
                .ToList();
        }
    }
}

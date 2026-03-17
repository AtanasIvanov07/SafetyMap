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
        private readonly IEmailQueueService _emailQueueService;

        public CrimeStatisticService(SafetyMapDbContext context, IEmailQueueService emailQueueService)
        {
            _context = context;
            _emailQueueService = emailQueueService;
        }

        public async Task<(IEnumerable<CrimeStatisticDTO> Statistics, int TotalCount)> GetAllAsync(string? neighborhoodSearch = null, string? categorySearch = null, int? year = null, int currentPage = 1, int itemsPerPage = 20)
        {
            var query = _context.CrimeStatistics
                .Include(c => c.Neighborhood)
                .Include(c => c.CrimeCategory)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(neighborhoodSearch))
            {
                var lowerSearch = neighborhoodSearch.ToLower();
                query = query.Where(c => c.Neighborhood != null && c.Neighborhood.Name.ToLower().Contains(lowerSearch));
            }

            if (!string.IsNullOrWhiteSpace(categorySearch))
            {
                var lowerSearch = categorySearch.ToLower();
                query = query.Where(c => c.CrimeCategory != null && c.CrimeCategory.Name.ToLower().Contains(lowerSearch));
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

        public async Task<IEnumerable<CrimeStatisticDTO>> GetUserSubscribedStatisticsAsync(string userId)
        {
            var userSubscribedNeighborhoodIds = await _context.UserSubscriptions
                .Where(us => us.UserId == userId)
                .Select(us => us.NeighborhoodId)
                .ToListAsync();

            if (!userSubscribedNeighborhoodIds.Any())
            {
                return new List<CrimeStatisticDTO>();
            }

            return await _context.CrimeStatistics
                .Include(c => c.Neighborhood)
                .Include(c => c.CrimeCategory)
                .Where(c => userSubscribedNeighborhoodIds.Contains(c.NeighborhoodId))
                .OrderByDescending(c => c.Year)
                .ThenBy(c => c.Neighborhood.Name)
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
            var trend = await CalculateTrendAsync(dto.NeighborhoodId, dto.CrimeCategoryId, dto.Year, dto.CountOfCrimes);

            var crimeStatistic = new CrimeStatistic
            {
                Id = Guid.NewGuid(),
                NeighborhoodId = dto.NeighborhoodId,
                CrimeCategoryId = dto.CrimeCategoryId,
                CountOfCrimes = dto.CountOfCrimes,
                Year = dto.Year,
                TrendPercentage = trend
            };

            _context.CrimeStatistics.Add(crimeStatistic);

            var nextYearStat = await _context.CrimeStatistics
                .FirstOrDefaultAsync(c => c.NeighborhoodId == dto.NeighborhoodId && c.CrimeCategoryId == dto.CrimeCategoryId && c.Year == dto.Year + 1);

            if (nextYearStat != null)
            {
                if (dto.CountOfCrimes > 0)
                {
                    nextYearStat.TrendPercentage = Math.Round(((double)(nextYearStat.CountOfCrimes - dto.CountOfCrimes) / dto.CountOfCrimes) * 100, 2);
                }
                else
                {
                    nextYearStat.TrendPercentage = 0;
                }
            }

            await _context.SaveChangesAsync();

          
            var subscribedUsersEmailQuery = from us in _context.UserSubscriptions
                                            join u in _context.ApplicationUsers on us.UserId equals u.Id
                                            where us.NeighborhoodId == dto.NeighborhoodId
                                            select u.Email;

            var userEmails = await subscribedUsersEmailQuery.ToListAsync();

            if (userEmails.Any())
            {
                var neighborhoodName = await _context.Neighborhoods
                    .Where(n => n.Id == dto.NeighborhoodId)
                    .Select(n => n.Name)
                    .FirstOrDefaultAsync() ?? "a neighborhood";

                foreach (var email in userEmails)
                {
                    if (!string.IsNullOrEmpty(email))
                    {
                        await _emailQueueService.QueueEmailAsync(new SafetyMap.Core.DTOs.Email.EmailPayload
                        {
                            ToEmail = email,
                            Subject = "New Safety Alert",
                            HtmlMessage = $"New safety activity has been reported in {neighborhoodName}, a neighborhood you are subscribed to."
                        });
                    }
                }
            }
        }

        public async Task UpdateAsync(CrimeStatisticEditDTO dto)
        {
            var crimeStatistic = await _context.CrimeStatistics.FindAsync(dto.Id);

            if (crimeStatistic == null)
            {
                throw new ArgumentException($"CrimeStatistic with Id '{dto.Id}' was not found.");
            }

            var oldNeighborhoodId = crimeStatistic.NeighborhoodId;
            var oldCategoryId = crimeStatistic.CrimeCategoryId;
            var oldYear = crimeStatistic.Year;

            crimeStatistic.NeighborhoodId = dto.NeighborhoodId;
            crimeStatistic.CrimeCategoryId = dto.CrimeCategoryId;
            crimeStatistic.CountOfCrimes = dto.CountOfCrimes;
            crimeStatistic.Year = dto.Year;

            crimeStatistic.TrendPercentage = await CalculateTrendAsync(dto.NeighborhoodId, dto.CrimeCategoryId, dto.Year, dto.CountOfCrimes);

            var newNextYearStat = await _context.CrimeStatistics
                .FirstOrDefaultAsync(c => c.NeighborhoodId == dto.NeighborhoodId && c.CrimeCategoryId == dto.CrimeCategoryId && c.Year == dto.Year + 1 && c.Id != dto.Id);

            if (newNextYearStat != null)
            {
                if (dto.CountOfCrimes > 0)
                {
                    newNextYearStat.TrendPercentage = Math.Round(((double)(newNextYearStat.CountOfCrimes - dto.CountOfCrimes) / dto.CountOfCrimes) * 100, 2);
                }
                else
                {
                    newNextYearStat.TrendPercentage = 0;
                }
            }

            if (oldNeighborhoodId != dto.NeighborhoodId || oldCategoryId != dto.CrimeCategoryId || oldYear != dto.Year)
            {
                var oldNextYearStat = await _context.CrimeStatistics
                    .FirstOrDefaultAsync(c => c.NeighborhoodId == oldNeighborhoodId && c.CrimeCategoryId == oldCategoryId && c.Year == oldYear + 1 && c.Id != dto.Id);

                if (oldNextYearStat != null)
                {
                    var oldNextYearPrev = await _context.CrimeStatistics
                        .Where(c => c.NeighborhoodId == oldNeighborhoodId && c.CrimeCategoryId == oldCategoryId && c.Year == oldYear && c.Id != dto.Id)
                        .Select(c => (int?)c.CountOfCrimes)
                        .FirstOrDefaultAsync();

                    if (oldNextYearPrev.HasValue && oldNextYearPrev.Value > 0)
                    {
                        oldNextYearStat.TrendPercentage = Math.Round(((double)(oldNextYearStat.CountOfCrimes - oldNextYearPrev.Value) / oldNextYearPrev.Value) * 100, 2);
                    }
                    else
                    {
                        oldNextYearStat.TrendPercentage = 0;
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var crimeStatistic = await _context.CrimeStatistics.FindAsync(id);

            if (crimeStatistic != null)
            {
                var nextYearStat = await _context.CrimeStatistics
                    .FirstOrDefaultAsync(c => c.NeighborhoodId == crimeStatistic.NeighborhoodId && c.CrimeCategoryId == crimeStatistic.CrimeCategoryId && c.Year == crimeStatistic.Year + 1);

                if (nextYearStat != null)
                {
                    var prevCount = await _context.CrimeStatistics
                        .Where(c => c.NeighborhoodId == crimeStatistic.NeighborhoodId && c.CrimeCategoryId == crimeStatistic.CrimeCategoryId && c.Year == crimeStatistic.Year && c.Id != id)
                        .Select(c => (int?)c.CountOfCrimes)
                        .FirstOrDefaultAsync();

                    if (prevCount.HasValue && prevCount.Value > 0)
                    {
                        nextYearStat.TrendPercentage = Math.Round(((double)(nextYearStat.CountOfCrimes - prevCount.Value) / prevCount.Value) * 100, 2);
                    }
                    else
                    {
                        nextYearStat.TrendPercentage = 0;
                    }
                }

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

        private async Task<double> CalculateTrendAsync(Guid neighborhoodId, Guid categoryId, int year, int countOfCrimes)
        {
            var prevCount = await _context.CrimeStatistics
                .Where(c => c.NeighborhoodId == neighborhoodId && c.CrimeCategoryId == categoryId && c.Year == year - 1)
                .Select(c => (int?)c.CountOfCrimes)
                .FirstOrDefaultAsync();

            if (prevCount.HasValue && prevCount.Value > 0)
            {
                return Math.Round(((double)(countOfCrimes - prevCount.Value) / prevCount.Value) * 100, 2);
            }
            return 0;
        }
    }
}

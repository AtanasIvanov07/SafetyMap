using Microsoft.EntityFrameworkCore;
using SafetyMap.Core.Contracts;
using SafetyMap.Core.DTOs.CrimeStatistic;
using SafetyMap.Core.DTOs.Email;
using SafetyMapData;
using SafetyMapData.Entities;
using System.Linq.Expressions;

namespace SafetyMap.Core.Services
{
    public class CrimeStatisticService : ICrimeStatisticService
    {
        private readonly SafetyMapDbContext _context;
        private readonly IEmailQueueService _emailQueueService;

        private const string NotAvailable = "N/A";
        private const string EmailSubject = "New Safety Alert";

        private static readonly Expression<Func<CrimeStatistic, CrimeStatisticDTO>> MapToDto = c => new CrimeStatisticDTO
        {
            Id = c.Id,
            NeighborhoodId = c.NeighborhoodId,
            NeighborhoodName = c.Neighborhood != null ? c.Neighborhood.Name : NotAvailable,
            CrimeCategoryId = c.CrimeCategoryId,
            CrimeCategoryName = c.CrimeCategory != null ? c.CrimeCategory.Name : NotAvailable,
            CountOfCrimes = c.CountOfCrimes,
            Year = c.Year,
            TrendPercentage = c.TrendPercentage
        };

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
                query = query.Where(c => c.Neighborhood != null && c.Neighborhood.Name.Contains(neighborhoodSearch));
            }

            if (!string.IsNullOrWhiteSpace(categorySearch))
            {
                query = query.Where(c => c.CrimeCategory != null && c.CrimeCategory.Name.Contains(categorySearch));
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
                .Select(MapToDto)
                .ToListAsync();

            return (statistics, totalCount);
        }

        public async Task<IEnumerable<CrimeStatisticDTO>> GetUserSubscribedStatisticsAsync(string userId)
        {
            return await _context.CrimeStatistics
                .Include(c => c.Neighborhood)
                .Include(c => c.CrimeCategory)
                .Where(c => _context.UserSubscriptions.Any(us => us.UserId == userId && us.NeighborhoodId == c.NeighborhoodId))
                .OrderByDescending(c => c.Year)
                .ThenBy(c => c.Neighborhood.Name)
                .Select(MapToDto)
                .ToListAsync();
        }

        public async Task<CrimeStatisticDTO?> GetByIdAsync(Guid id)
        {
            return await _context.CrimeStatistics
                .Include(c => c.Neighborhood)
                .Include(c => c.CrimeCategory)
                .Where(c => c.Id == id)
                .Select(MapToDto)
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
                TrendPercentage = 0
            };

            _context.CrimeStatistics.Add(crimeStatistic);
            await _context.SaveChangesAsync();

            await UpdateTrendForYearAsync(dto.NeighborhoodId, dto.CrimeCategoryId, dto.Year);
            await UpdateTrendForYearAsync(dto.NeighborhoodId, dto.CrimeCategoryId, dto.Year + 1);
            await _context.SaveChangesAsync();

            await NotifySubscribersAsync(dto.NeighborhoodId);
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

            await _context.SaveChangesAsync();

            await UpdateTrendForYearAsync(dto.NeighborhoodId, dto.CrimeCategoryId, dto.Year);
            await UpdateTrendForYearAsync(dto.NeighborhoodId, dto.CrimeCategoryId, dto.Year + 1);

            if (oldNeighborhoodId != dto.NeighborhoodId || oldCategoryId != dto.CrimeCategoryId || oldYear != dto.Year)
            {
                await UpdateTrendForYearAsync(oldNeighborhoodId, oldCategoryId, oldYear + 1);
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var crimeStatistic = await _context.CrimeStatistics.FindAsync(id);

            if (crimeStatistic != null)
            {
                var neighborhoodId = crimeStatistic.NeighborhoodId;
                var categoryId = crimeStatistic.CrimeCategoryId;
                var year = crimeStatistic.Year;

                _context.CrimeStatistics.Remove(crimeStatistic);
                await _context.SaveChangesAsync();

                await UpdateTrendForYearAsync(neighborhoodId, categoryId, year + 1);
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

        private async Task UpdateTrendForYearAsync(Guid neighborhoodId, Guid categoryId, int targetYear)
        {
            var targetStat = await _context.CrimeStatistics
                .FirstOrDefaultAsync(c => c.NeighborhoodId == neighborhoodId && c.CrimeCategoryId == categoryId && c.Year == targetYear);

            if (targetStat == null) return;

            var prevCount = await _context.CrimeStatistics
                .Where(c => c.NeighborhoodId == neighborhoodId && c.CrimeCategoryId == categoryId && c.Year == targetYear - 1)
                .Select(c => (int?)c.CountOfCrimes)
                .FirstOrDefaultAsync();

            if (prevCount.HasValue && prevCount.Value > 0)
            {
                targetStat.TrendPercentage = Math.Round(((double)(targetStat.CountOfCrimes - prevCount.Value) / prevCount.Value) * 100, 2);
            }
            else
            {
                targetStat.TrendPercentage = 0;
            }
        }

        private async Task NotifySubscribersAsync(Guid neighborhoodId)
        {
            var subscribedUsersEmailQuery = from us in _context.UserSubscriptions
                                            join u in _context.ApplicationUsers on us.UserId equals u.Id
                                            where us.NeighborhoodId == neighborhoodId
                                            select u.Email;

            var userEmails = await subscribedUsersEmailQuery.ToListAsync();

            if (userEmails.Any())
            {
                var neighborhoodName = await _context.Neighborhoods
                    .Where(n => n.Id == neighborhoodId)
                    .Select(n => n.Name)
                    .FirstOrDefaultAsync() ?? "a neighborhood";

                foreach (var email in userEmails.Where(e => !string.IsNullOrEmpty(e)))
                {
                    await _emailQueueService.QueueEmailAsync(new EmailPayload
                    {
                        ToEmail = email,
                        Subject = EmailSubject,
                        HtmlMessage = $"New safety activity has been reported in {neighborhoodName}, a neighborhood you are subscribed to."
                    });
                }
            }
        }
    }
}
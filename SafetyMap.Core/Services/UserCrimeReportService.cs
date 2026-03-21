using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SafetyMap.Core.Contracts;
using SafetyMap.Core.DTOs;
using SafetyMap.Core.DTOs.CrimeStatistic;
using SafetyMapData;
using SafetyMapData.Entities;
using SafetyMapData.Enums;

namespace SafetyMap.Core.Services
{
    public class UserCrimeReportService : IUserCrimeReportService
    {
        private readonly SafetyMapDbContext _context;
        private readonly ICrimeStatisticService _crimeStatisticService;

        public UserCrimeReportService(SafetyMapDbContext context, ICrimeStatisticService crimeStatisticService)
        {
            _context = context;
            _crimeStatisticService = crimeStatisticService;
        }

        public async Task SubmitReportAsync(UserCrimeReportCreateDTO dto, string userId)
        {
            var report = new UserCrimeReport
            {
                Id = Guid.NewGuid(),
                Description = dto.Description,
                DateOfIncident = dto.DateOfIncident,
                CrimeCategoryId = dto.CrimeCategoryId,
                CityId = dto.CityId,
                NeighborhoodId = dto.NeighborhoodId,
                UserId = userId,
                Status = ReportStatus.Pending
            };

            _context.UserCrimeReports.Add(report);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<PendingReportDTO>> GetPendingReportsAsync()
        {
            return await _context.UserCrimeReports
                .Include(r => r.CrimeCategory)
                .Include(r => r.City)
                .Include(r => r.Neighborhood)
                .Include(r => r.UserIdentity)
                .Where(r => r.Status == ReportStatus.Pending)
                .OrderBy(r => r.DateOfIncident)
                .Select(r => new PendingReportDTO
                {
                    Id = r.Id,
                    Description = r.Description,
                    DateOfIncident = r.DateOfIncident,
                    CrimeCategoryName = r.CrimeCategory.Name,
                    CityName = r.City.Name,
                    NeighborhoodName = r.Neighborhood != null ? r.Neighborhood.Name : null,
                    ReporterName = r.UserIdentity.FirstName + " " + r.UserIdentity.LastName,
                    ReporterEmail = r.UserIdentity.Email
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<PendingReportDTO>> GetReportsByUserAsync(string userId)
        {
            return await _context.UserCrimeReports
                .Include(r => r.CrimeCategory)
                .Include(r => r.City)
                .Include(r => r.Neighborhood)
                .Include(r => r.UserIdentity)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.DateOfIncident)
                .Select(r => new PendingReportDTO
                {
                    Id = r.Id,
                    Description = r.Description,
                    DateOfIncident = r.DateOfIncident,
                    CrimeCategoryName = r.CrimeCategory.Name,
                    CityName = r.City.Name,
                    NeighborhoodName = r.Neighborhood != null ? r.Neighborhood.Name : null,
                    ReporterName = r.UserIdentity.FirstName + " " + r.UserIdentity.LastName,
                    ReporterEmail = r.UserIdentity.Email
                })
                .ToListAsync();
        }

        public async Task ApproveReportAsync(Guid reportId)
        {
            var report = await _context.UserCrimeReports.FindAsync(reportId);
            if (report == null || report.Status != ReportStatus.Pending)
            {
                throw new ArgumentException("Invalid report or report is not pending.");
            }

            report.Status = ReportStatus.Approved;
            await _context.SaveChangesAsync(); // Save the report status

            //ako ima neighborhood, add to statistics
            if (report.NeighborhoodId.HasValue)
            {
                var year = report.DateOfIncident.Year;
                var stat = await _context.CrimeStatistics
                    .FirstOrDefaultAsync(s => s.NeighborhoodId == report.NeighborhoodId.Value &&
                                              s.CrimeCategoryId == report.CrimeCategoryId &&
                                              s.Year == year);

                if (stat != null)
                {
                    var editDto = new CrimeStatisticEditDTO
                    {
                        Id = stat.Id,
                        NeighborhoodId = stat.NeighborhoodId,
                        CrimeCategoryId = stat.CrimeCategoryId,
                        Year = stat.Year,
                        CountOfCrimes = stat.CountOfCrimes + 1,
                        TrendPercentage = stat.TrendPercentage
                    };
                    await _crimeStatisticService.UpdateAsync(editDto);
                }
                else
                {
                    var createDto = new CrimeStatisticCreateDTO
                    {
                        NeighborhoodId = report.NeighborhoodId.Value,
                        CrimeCategoryId = report.CrimeCategoryId,
                        Year = year,
                        CountOfCrimes = 1,
                        TrendPercentage = 0
                    };
                    await _crimeStatisticService.CreateAsync(createDto);
                }
            }
        }

        public async Task RejectReportAsync(Guid reportId)
        {
            var report = await _context.UserCrimeReports.FindAsync(reportId);
            if (report == null || report.Status != ReportStatus.Pending)
            {
                throw new ArgumentException("Invalid report or report is not pending.");
            }

            report.Status = ReportStatus.Rejected;
            await _context.SaveChangesAsync();
        }
    }
}

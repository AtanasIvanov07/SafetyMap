using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SafetyMap.Core.Contracts;
using SafetyMap.Core.DTOs;
using SafetyMap.Core.DTOs.CrimeStatistic;
using SafetyMap.Core.DTOs.Email;
using SafetyMap.Core.DTOs.UserCrimeReport;
using SafetyMapData;
using SafetyMapData.Entities;
using SafetyMapData.Enums;

namespace SafetyMap.Core.Services
{
    public class UserCrimeReportService : IUserCrimeReportService
    {
        private readonly SafetyMapDbContext _context;
        private readonly ICrimeStatisticService _crimeStatisticService;
        private readonly IEmailQueueService _emailQueueService;
        private readonly IPhotoService _photoService;
        private readonly ILogger<UserCrimeReportService> _logger;

        public UserCrimeReportService(
            SafetyMapDbContext context,
            ICrimeStatisticService crimeStatisticService,
            IEmailQueueService emailQueueService,
            IPhotoService photoService,
            ILogger<UserCrimeReportService> logger)
        {
            _context = context;
            _crimeStatisticService = crimeStatisticService;
            _emailQueueService = emailQueueService;
            _photoService = photoService;
            _logger = logger;
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

            foreach (var url in dto.ImageUrls)
            {
                report.Images.Add(new UserCrimeReportImage
                {
                    Id = Guid.NewGuid(),
                    ImageUrl = url,
                    UserCrimeReportId = report.Id
                });
            }

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
                .Include(r => r.Images)
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
                    ReporterEmail = r.UserIdentity.Email,
                    Status = r.Status.ToString(),
                    Images = r.Images.Select(i => new ReportImageDTO { Id = i.Id, ImageUrl = i.ImageUrl }).ToList()
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
                .Include(r => r.Images)
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
                    ReporterEmail = r.UserIdentity.Email,
                    Status = r.Status.ToString(),
                    Images = r.Images.Select(i => new ReportImageDTO { Id = i.Id, ImageUrl = i.ImageUrl }).ToList()
                })
                .ToListAsync();
        }

        public async Task<bool> DeleteImageAsync(Guid imageId, string userId)
        {
            var image = await _context.UserCrimeReportImages
                .Include(i => i.UserCrimeReport)
                .FirstOrDefaultAsync(i => i.Id == imageId);

            if (image == null)
                return false;

            // Only allow the report owner to delete images, and only on pending reports
            if (image.UserCrimeReport.UserId != userId || image.UserCrimeReport.Status != ReportStatus.Pending)
                return false;

            // Delete from Cloudinary
            await _photoService.DeletePhotoAsync(image.ImageUrl);

            _context.UserCrimeReportImages.Remove(image);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task ApproveReportAsync(Guid reportId)
        {
            var report = await _context.UserCrimeReports
                .Include(r => r.UserIdentity)
                .FirstOrDefaultAsync(r => r.Id == reportId);

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

            // Queue Email Notification
            if (report.UserIdentity != null && !string.IsNullOrEmpty(report.UserIdentity.Email))
            {
                try
                {
                    var payload = new EmailPayload
                    {
                        ToEmail = report.UserIdentity.Email,
                        Subject = "Your SafetyMap Crime Report was Approved",
                        HtmlMessage = $"<p>Hello {report.UserIdentity.FirstName},</p><p>Thank you for your contribution! Your recent crime report submitted on {report.DateOfIncident.ToShortDateString()} has been reviewed and approved by our administrators. It has now been added to the community statistics map.</p><p>Stay safe!</p>"
                    };
                    await _emailQueueService.QueueEmailAsync(payload);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to queue approval email for user {UserId} regarding report {ReportId}", report.UserId, reportId);
                }
            }
        }

        public async Task RejectReportAsync(Guid reportId)
        {
            var report = await _context.UserCrimeReports
                .Include(r => r.UserIdentity)
                .Include(r => r.Images)
                .FirstOrDefaultAsync(r => r.Id == reportId);

            if (report == null || report.Status != ReportStatus.Pending)
            {
                throw new ArgumentException("Invalid report or report is not pending.");
            }

            // ako admin rejectne reporta, da se iztriqt snimkite ot cloudinary
            foreach (var image in report.Images)
            {
                await _photoService.DeletePhotoAsync(image.ImageUrl);
            }

            _context.UserCrimeReportImages.RemoveRange(report.Images);

            report.Status = ReportStatus.Rejected;
            await _context.SaveChangesAsync();

            // Queue Email Notification
            if (report.UserIdentity != null && !string.IsNullOrEmpty(report.UserIdentity.Email))
            {
                try
                {
                    var payload = new EmailPayload
                    {
                        ToEmail = report.UserIdentity.Email,
                        Subject = "Update on your SafetyMap Crime Report",
                        HtmlMessage = $"<p>Hello {report.UserIdentity.FirstName},</p><p>We have reviewed your recent crime report submitted on {report.DateOfIncident.ToShortDateString()}. Unfortunately, it has not been published (e.g., due to lack of details or being a duplicate).</p><p>We still appreciate your effort to keep the community safe. Continue staying safe!</p>"
                    };
                    await _emailQueueService.QueueEmailAsync(payload);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to queue rejection email for user {UserId} regarding report {ReportId}", report.UserId, reportId);
                }
            }
        }
    }
}

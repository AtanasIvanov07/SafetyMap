using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SafetyMap.Core.DTOs;
using SafetyMap.Core.DTOs.UserCrimeReport;

namespace SafetyMap.Core.Contracts
{
    public interface IUserCrimeReportService
    {
        Task SubmitReportAsync(UserCrimeReportCreateDTO dto, string userId);
        Task<IEnumerable<PendingReportDTO>> GetPendingReportsAsync();
        Task<IEnumerable<PendingReportDTO>> GetReportsByUserAsync(string userId);
        Task ApproveReportAsync(Guid reportId);
        Task RejectReportAsync(Guid reportId);
    }
}

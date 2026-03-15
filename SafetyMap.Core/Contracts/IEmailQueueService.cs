using SafetyMap.Core.DTOs.Email;
using System.Threading;
using System.Threading.Tasks;

namespace SafetyMap.Core.Contracts
{
    public interface IEmailQueueService
    {
        ValueTask QueueEmailAsync(EmailPayload payload);
        ValueTask<EmailPayload> DequeueAsync(CancellationToken token);
    }
}

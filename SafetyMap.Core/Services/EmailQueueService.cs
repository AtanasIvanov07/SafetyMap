using SafetyMap.Core.Contracts;
using SafetyMap.Core.DTOs.Email;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace SafetyMap.Core.Services
{
    public class EmailQueueService : IEmailQueueService
    {
        private readonly Channel<EmailPayload> _queue;

        public EmailQueueService()
        {
            var options = new BoundedChannelOptions(100)
            {
                FullMode = BoundedChannelFullMode.Wait
            };
            _queue = Channel.CreateBounded<EmailPayload>(options);
        }

        public async ValueTask QueueEmailAsync(EmailPayload payload)
        {
            await _queue.Writer.WriteAsync(payload);
        }

        public async ValueTask<EmailPayload> DequeueAsync(CancellationToken token)
        {
            return await _queue.Reader.ReadAsync(token);
        }
    }
}

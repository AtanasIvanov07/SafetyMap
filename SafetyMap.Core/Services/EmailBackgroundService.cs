using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using SafetyMap.Core.Contracts;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SafetyMap.Core.Services
{
    public class EmailBackgroundService : BackgroundService
    {
        private readonly IEmailQueueService _emailQueue;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EmailBackgroundService> _logger;

        public EmailBackgroundService(IEmailQueueService emailQueue, IServiceProvider serviceProvider, ILogger<EmailBackgroundService> logger)
        {
            _emailQueue = emailQueue;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Email Background Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var emailPayload = await _emailQueue.DequeueAsync(stoppingToken);

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                        await emailService.SendEmailAsync(emailPayload.ToEmail, emailPayload.Subject, emailPayload.HtmlMessage);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Prevent throwing if stoppingToken was signaled
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing Email Background Service.");
                }
            }

            _logger.LogInformation("Email Background Service is stopping.");
        }
    }
}

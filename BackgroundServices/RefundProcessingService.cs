using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Assignment_Example_HU.Services.Interfaces;

namespace Assignment_Example_HU.BackgroundServices
{
    public class RefundProcessingService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RefundProcessingService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5); // Check every 5 minutes

        public RefundProcessingService(
            IServiceProvider serviceProvider,
            ILogger<RefundProcessingService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("RefundProcessingService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessPendingRefundsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing refunds.");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("RefundProcessingService stopped.");
        }

        private async Task ProcessPendingRefundsAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var refundService = scope.ServiceProvider.GetRequiredService<IRefundService>();

            try
            {
                _logger.LogInformation("Processing pending refunds...");
                await refundService.ProcessPendingRefundsAsync();
                _logger.LogInformation("Refund processing completed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ProcessPendingRefundsAsync");
                throw;
            }
        }
    }
}

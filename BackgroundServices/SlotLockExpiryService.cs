using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Assignment_Example_HU.Services.Interfaces;

namespace Assignment_Example_HU.BackgroundServices
{
    public class SlotLockExpiryService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SlotLockExpiryService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1); // Check every minute

        public SlotLockExpiryService(
            IServiceProvider serviceProvider,
            ILogger<SlotLockExpiryService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("SlotLockExpiryService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ExpireLocksAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while expiring slot locks.");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("SlotLockExpiryService stopped.");
        }

        private async Task ExpireLocksAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var slotService = scope.ServiceProvider.GetRequiredService<ISlotService>();

            try
            {
                await slotService.ExpireLocksAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ExpireLocksAsync");
                throw;
            }
        }
    }
}

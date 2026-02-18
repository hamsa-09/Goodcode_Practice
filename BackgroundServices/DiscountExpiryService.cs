using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Assignment_Example_HU.Data;
using Assignment_Example_HU.Repositories.Interfaces;

namespace Assignment_Example_HU.BackgroundServices
{
    public class DiscountExpiryService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DiscountExpiryService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1); // Check every hour

        public DiscountExpiryService(
            IServiceProvider serviceProvider,
            ILogger<DiscountExpiryService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("DiscountExpiryService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ExpireDiscountsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while expiring discounts.");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("DiscountExpiryService stopped.");
        }

        private async Task ExpireDiscountsAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var discountRepository = scope.ServiceProvider.GetRequiredService<IDiscountRepository>();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            try
            {
                var now = DateTime.UtcNow;
                
                // Get all active discounts that have expired
                var expiredDiscounts = await dbContext.Discounts
                    .Where(d => d.IsActive && d.ValidTo < now)
                    .ToListAsync();

                foreach (var discount in expiredDiscounts)
                {
                    discount.IsActive = false;
                    await discountRepository.UpdateAsync(discount);
                }

                if (expiredDiscounts.Any())
                {
                    await discountRepository.SaveChangesAsync();
                    _logger.LogInformation($"Deactivated {expiredDiscounts.Count} expired discounts.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ExpireDiscountsAsync");
                throw;
            }
        }
    }
}

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Assignment_Example_HU.Data;
using Assignment_Example_HU.Enums;
using Assignment_Example_HU.Services.Interfaces;

namespace Assignment_Example_HU.BackgroundServices
{
    public class WaitlistCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<WaitlistCleanupService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(10); // Check every 10 minutes

        public WaitlistCleanupService(
            IServiceProvider serviceProvider,
            ILogger<WaitlistCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("WaitlistCleanupService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupWaitlistsForStartedGamesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while cleaning up waitlists.");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("WaitlistCleanupService stopped.");
        }

        private async Task CleanupWaitlistsForStartedGamesAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var waitlistService = scope.ServiceProvider.GetRequiredService<IWaitlistService>();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            try
            {
                var now = DateTime.UtcNow;

                // Find games that have started (slot start time has passed) and are confirmed/completed
                var startedGames = await dbContext.Games
                    .Include(g => g.Slot)
                    .Where(g => g.Status == GameStatus.Confirmed || g.Status == GameStatus.Completed)
                    .Where(g => g.Slot.StartTime <= now)
                    .ToListAsync();

                foreach (var game in startedGames)
                {
                    _logger.LogInformation($"Removing waitlist for game {game.Id} (game has started).");
                    await waitlistService.RemoveWaitlistForGameAsync(game.Id);
                }

                if (startedGames.Any())
                {
                    _logger.LogInformation($"Cleaned up waitlists for {startedGames.Count} started games.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CleanupWaitlistsForStartedGamesAsync");
                throw;
            }
        }
    }
}

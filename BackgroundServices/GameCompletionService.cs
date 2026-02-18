using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Assignment_Example_HU.Services.Interfaces;

namespace Assignment_Example_HU.BackgroundServices
{
    public class GameCompletionService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<GameCompletionService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(10); // Check every 10 minutes

        public GameCompletionService(
            IServiceProvider serviceProvider,
            ILogger<GameCompletionService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("GameCompletionService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessGameCompletionAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing game completion.");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("GameCompletionService stopped.");
        }

        private async Task ProcessGameCompletionAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var gameService = scope.ServiceProvider.GetRequiredService<IGameService>();

            try
            {
                _logger.LogInformation("Checking for games to mark as completed...");
                await gameService.CompleteGamesAsync();
                _logger.LogInformation("Game completion check finished.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ProcessGameCompletionAsync");
                throw;
            }
        }
    }
}

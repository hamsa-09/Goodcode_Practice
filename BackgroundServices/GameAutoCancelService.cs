using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Assignment_Example_HU.Services.Interfaces;

namespace Assignment_Example_HU.BackgroundServices
{
    public class GameAutoCancelService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<GameAutoCancelService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(15); // Check every 15 minutes

        public GameAutoCancelService(
            IServiceProvider serviceProvider,
            ILogger<GameAutoCancelService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("GameAutoCancelService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessPendingGamesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing pending games.");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("GameAutoCancelService stopped.");
        }

        private async Task ProcessPendingGamesAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var gameService = scope.ServiceProvider.GetRequiredService<IGameService>();

            try
            {
                _logger.LogInformation("Checking for games with low player count...");
                await gameService.CancelGamesWithLowPlayersAsync();
                _logger.LogInformation("Game auto-cancel check completed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ProcessPendingGamesAsync");
                throw;
            }
        }
    }
}

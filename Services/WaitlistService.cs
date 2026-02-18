using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using AutoMapper;
using Assignment_Example_HU.DTOs;
using Assignment_Example_HU.Models;
using Assignment_Example_HU.Repositories.Interfaces;
using Assignment_Example_HU.Services.Interfaces;

namespace Assignment_Example_HU.Services
{
    public class WaitlistService : IWaitlistService
    {
        private readonly IWaitlistRepository _waitlistRepository;
        private readonly IGameRepository _gameRepository;
        private readonly UserManager<User> _userManager;
        private readonly IRatingRepository _ratingRepository;
        private readonly IGamePlayerRepository _gamePlayerRepository;
        private readonly IMapper _mapper;
        private readonly int _maxWaitlistSize;

        public WaitlistService(
            IWaitlistRepository waitlistRepository,
            IGameRepository gameRepository,
            UserManager<User> userManager,
            IRatingRepository ratingRepository,
            IGamePlayerRepository gamePlayerRepository,
            IConfiguration configuration,
            IMapper mapper)
        {
            _waitlistRepository = waitlistRepository;
            _gameRepository = gameRepository;
            _userManager = userManager;
            _ratingRepository = ratingRepository;
            _gamePlayerRepository = gamePlayerRepository;
            _mapper = mapper;
            _maxWaitlistSize = configuration.GetValue<int>("Waitlist:MaxSize", 20);
        }

        public async Task<WaitlistDto> JoinWaitlistAsync(Guid gameId, Guid userId)
        {
            var game = await _gameRepository.GetByIdAsync(gameId);
            if (game == null)
            {
                throw new InvalidOperationException("Game not found.");
            }

            if (game.Status != Enums.GameStatus.Pending)
            {
                throw new InvalidOperationException("Can only join waitlist for pending games.");
            }

            // Check if user is already in the game
            var gameWithPlayers = await _gameRepository.GetByIdWithPlayersAsync(gameId);
            if (gameWithPlayers != null && gameWithPlayers.Players.Any(p => p.UserId == userId))
            {
                throw new InvalidOperationException("You are already in this game.");
            }

            // Check if already in waitlist
            var existing = await _waitlistRepository.GetByGameAndUserAsync(gameId, userId);
            if (existing != null)
            {
                return _mapper.Map<WaitlistDto>(existing);
            }

            // Check waitlist size
            var currentCount = await _waitlistRepository.GetCountByGameIdAsync(gameId);
            if (currentCount >= _maxWaitlistSize)
            {
                throw new InvalidOperationException($"Waitlist is full. Maximum size: {_maxWaitlistSize}");
            }

            // Get player's aggregated rating
            var user = await _userManager.FindByIdAsync(userId.ToString());
            var playerRating = user?.AggregatedRating ?? 0;

            // Calculate priority (higher rating = higher priority, then by join time)
            var priority = CalculatePriority(playerRating, currentCount);

            var waitlist = new Waitlist
            {
                Id = Guid.NewGuid(),
                GameId = gameId,
                UserId = userId,
                PlayerRating = playerRating,
                Priority = priority
            };

            await _waitlistRepository.AddAsync(waitlist);
            await _waitlistRepository.SaveChangesAsync();

            var waitlistWithUser = await _waitlistRepository.GetByIdAsync(waitlist.Id);
            return _mapper.Map<WaitlistDto>(waitlistWithUser!);
        }

        public async Task<bool> LeaveWaitlistAsync(Guid gameId, Guid userId)
        {
            var waitlist = await _waitlistRepository.GetByGameAndUserAsync(gameId, userId);
            if (waitlist == null)
            {
                return false;
            }

            await _waitlistRepository.RemoveAsync(waitlist);
            await _waitlistRepository.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<WaitlistDto>> GetWaitlistByGameAsync(Guid gameId)
        {
            var waitlists = await _waitlistRepository.GetByGameIdAsync(gameId);
            return _mapper.Map<IEnumerable<WaitlistDto>>(waitlists);
        }

        public async Task<bool> InviteFromWaitlistAsync(Guid gameId, Guid userId, Guid invitedUserId)
        {
            // Only game creator can invite
            var game = await _gameRepository.GetByIdAsync(gameId);
            if (game == null || game.CreatedByUserId != userId)
            {
                throw new UnauthorizedAccessException("Only the game creator can invite from waitlist.");
            }

            var gameWithPlayers = await _gameRepository.GetByIdWithPlayersAsync(gameId);
            if (gameWithPlayers == null)
            {
                throw new InvalidOperationException("Game not found.");
            }

            if (gameWithPlayers.Players.Count >= gameWithPlayers.MaxPlayers)
            {
                throw new InvalidOperationException("Game is full.");
            }

            // Check if user is in waitlist
            var waitlist = await _waitlistRepository.GetByGameAndUserAsync(gameId, invitedUserId);
            if (waitlist == null)
            {
                throw new InvalidOperationException("User is not in the waitlist.");
            }

            // Add user to game
            var player = new GamePlayer
            {
                Id = Guid.NewGuid(),
                GameId = gameId,
                UserId = invitedUserId
            };

            await _gamePlayerRepository.AddAsync(player);
            await _gamePlayerRepository.SaveChangesAsync();

            // Remove from waitlist
            await _waitlistRepository.RemoveAsync(waitlist);
            await _waitlistRepository.SaveChangesAsync();

            return true;
        }

        public async Task RemoveWaitlistForGameAsync(Guid gameId)
        {
            var waitlists = await _waitlistRepository.GetByGameIdAsync(gameId);
            foreach (var waitlist in waitlists)
            {
                await _waitlistRepository.RemoveAsync(waitlist);
            }
            await _waitlistRepository.SaveChangesAsync();
        }

        public async Task<int> GetWaitlistCountAsync(Guid gameId)
        {
            return await _waitlistRepository.GetCountByGameIdAsync(gameId);
        }

        private static int CalculatePriority(decimal playerRating, int currentPosition)
        {
            // Priority = (rating * 1000) - currentPosition
            // Higher rating = higher priority
            // If same rating, earlier join = higher priority
            return (int)(playerRating * 1000) - currentPosition;
        }
    }
}

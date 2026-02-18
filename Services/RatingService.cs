using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using AutoMapper;
using Assignment_Example_HU.Data;
using Assignment_Example_HU.DTOs;
using Assignment_Example_HU.Enums;
using Assignment_Example_HU.Models;
using Assignment_Example_HU.Repositories.Interfaces;
using Assignment_Example_HU.Services.Interfaces;

namespace Assignment_Example_HU.Services
{
    public class RatingService : IRatingService
    {
        private readonly IRatingRepository _ratingRepository;
        private readonly IGameRepository _gameRepository;
        private readonly UserManager<User> _userManager;
        private readonly IVenueRepository _venueRepository;
        private readonly ICourtRepository _courtRepository;
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;

        public RatingService(
            IRatingRepository ratingRepository,
            IGameRepository gameRepository,
            UserManager<User> userManager,
            IVenueRepository venueRepository,
            ICourtRepository courtRepository,
            AppDbContext dbContext,
            IMapper mapper)
        {
            _ratingRepository = ratingRepository;
            _gameRepository = gameRepository;
            _userManager = userManager;
            _venueRepository = venueRepository;
            _courtRepository = courtRepository;
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<RatingDto> CreateRatingAsync(Guid userId, CreateRatingDto dto)
        {
            if (dto.Score < 1 || dto.Score > 5)
            {
                throw new InvalidOperationException("Rating score must be between 1 and 5.");
            }

            // Validate based on type
            switch (dto.Type)
            {
                case RatingType.Venue:
                    if (!dto.VenueId.HasValue)
                    {
                        throw new InvalidOperationException("VenueId is required for venue ratings.");
                    }
                    var venue = await _venueRepository.GetByIdAsync(dto.VenueId.Value);
                    if (venue == null)
                    {
                        throw new InvalidOperationException("Venue not found.");
                    }
                    break;

                case RatingType.Court:
                    if (!dto.CourtId.HasValue)
                    {
                        throw new InvalidOperationException("CourtId is required for court ratings.");
                    }
                    var court = await _courtRepository.GetByIdAsync(dto.CourtId.Value);
                    if (court == null)
                    {
                        throw new InvalidOperationException("Court not found.");
                    }
                    break;

                case RatingType.Player:
                    if (!dto.PlayerId.HasValue || !dto.GameId.HasValue)
                    {
                        throw new InvalidOperationException("PlayerId and GameId are required for player ratings.");
                    }

                    // Check if game is completed
                    var game = await _gameRepository.GetByIdAsync(dto.GameId.Value);
                    if (game == null)
                    {
                        throw new InvalidOperationException("Game not found.");
                    }

                    if (game.Status != GameStatus.Completed)
                    {
                        throw new InvalidOperationException("Can only rate players after game completion.");
                    }

                    // Check if user participated in this game
                    var gameWithPlayers = await _gameRepository.GetByIdWithPlayersAsync(dto.GameId.Value);
                    if (gameWithPlayers == null || !gameWithPlayers.Players.Any(p => p.UserId == userId))
                    {
                        throw new UnauthorizedAccessException("You can only rate players from games you participated in.");
                    }

                    // Check if already rated this player for this game
                    var existingRating = await _ratingRepository.GetByUserAndGameAndTypeAsync(
                        userId, dto.GameId.Value, RatingType.Player);
                    if (existingRating != null && existingRating.PlayerId == dto.PlayerId)
                    {
                        throw new InvalidOperationException("You have already rated this player for this game.");
                    }
                    break;
            }

            var rating = new Rating
            {
                Id = Guid.NewGuid(),
                Type = dto.Type,
                RatedById = userId,
                VenueId = dto.VenueId,
                CourtId = dto.CourtId,
                PlayerId = dto.PlayerId,
                GameId = dto.GameId,
                Score = dto.Score,
                Comment = dto.Comment
            };

            await _ratingRepository.AddAsync(rating);
            await _ratingRepository.SaveChangesAsync();

            // Update player aggregated rating if it's a player rating
            if (dto.Type == RatingType.Player && dto.PlayerId.HasValue)
            {
                await UpdatePlayerAggregatedRatingAsync(dto.PlayerId.Value);
            }

            var ratingWithDetails = await _ratingRepository.GetByIdAsync(rating.Id);
            return _mapper.Map<RatingDto>(ratingWithDetails!);
        }

        public async Task<IEnumerable<RatingDto>> GetVenueRatingsAsync(Guid venueId)
        {
            var ratings = await _ratingRepository.GetByVenueIdAsync(venueId);
            return _mapper.Map<IEnumerable<RatingDto>>(ratings);
        }

        public async Task<IEnumerable<RatingDto>> GetCourtRatingsAsync(Guid courtId)
        {
            var ratings = await _ratingRepository.GetByCourtIdAsync(courtId);
            return _mapper.Map<IEnumerable<RatingDto>>(ratings);
        }

        public async Task<IEnumerable<RatingDto>> GetPlayerRatingsAsync(Guid playerId)
        {
            var ratings = await _ratingRepository.GetByPlayerIdAsync(playerId);
            return _mapper.Map<IEnumerable<RatingDto>>(ratings);
        }

        public async Task<PlayerProfileDto> GetPlayerProfileAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            // Get recent reviews (last 10)
            var playerRatings = await _ratingRepository.GetByPlayerIdAsync(userId);
            var recentReviews = playerRatings
                .OrderByDescending(r => r.CreatedAt)
                .Take(10)
                .Select(r => new RecentReviewDto
                {
                    RatingId = r.Id,
                    Score = r.Score,
                    Comment = r.Comment,
                    RatedByName = r.RatedBy?.UserName ?? "Unknown",
                    CreatedAt = r.CreatedAt
                })
                .ToList();

            // Get games played count
            var gamesPlayed = await _dbContext.GamePlayers
                .CountAsync(gp => gp.UserId == userId);

            // Get preferred sports (from games played)
            var preferredSports = await _dbContext.GamePlayers
                .Include(gp => gp.Game)
                    .ThenInclude(g => g.Slot)
                        .ThenInclude(s => s.Court)
                .Where(gp => gp.UserId == userId)
                .Select(gp => gp.Game.Slot.Court.SportType.ToString())
                .Distinct()
                .ToListAsync();

            return new PlayerProfileDto
            {
                UserId = user.Id,
                UserName = user.UserName,
                AggregatedRating = user.AggregatedRating,
                GamesPlayed = gamesPlayed,
                PreferredSports = string.Join(", ", preferredSports),
                RecentReviews = recentReviews
            };
        }

        public async Task<VenueRatingSummaryDto> GetVenueRatingSummaryAsync(Guid venueId)
        {
            var ratings = await _ratingRepository.GetByVenueIdAsync(venueId);
            var venue = await _venueRepository.GetByIdAsync(venueId);

            if (!ratings.Any())
            {
                return new VenueRatingSummaryDto
                {
                    VenueId = venueId,
                    VenueName = venue?.Name ?? "Unknown",
                    AverageRating = 0,
                    TotalRatings = 0
                };
            }

            var averageRating = ratings.Average(r => r.Score);

            return new VenueRatingSummaryDto
            {
                VenueId = venueId,
                VenueName = venue?.Name ?? "Unknown",
                AverageRating = (decimal)Math.Round(averageRating, 2),
                TotalRatings = ratings.Count()
            };
        }

        public async Task<CourtRatingSummaryDto> GetCourtRatingSummaryAsync(Guid courtId)
        {
            var ratings = await _ratingRepository.GetByCourtIdAsync(courtId);
            var court = await _courtRepository.GetByIdAsync(courtId);

            if (!ratings.Any())
            {
                return new CourtRatingSummaryDto
                {
                    CourtId = courtId,
                    CourtName = court?.Name ?? "Unknown",
                    AverageRating = 0,
                    TotalRatings = 0
                };
            }

            var averageRating = ratings.Average(r => r.Score);

            return new CourtRatingSummaryDto
            {
                CourtId = courtId,
                CourtName = court?.Name ?? "Unknown",
                AverageRating = (decimal)Math.Round(averageRating, 2),
                TotalRatings = ratings.Count()
            };
        }

        public async Task UpdatePlayerAggregatedRatingAsync(Guid playerId)
        {
            var ratings = await _ratingRepository.GetByPlayerIdAsync(playerId);

            if (!ratings.Any())
            {
                return;
            }

            var averageRating = ratings.Average(r => r.Score);
            var user = await _userManager.FindByIdAsync(playerId.ToString());

            if (user != null)
            {
                user.AggregatedRating = (decimal)Math.Round(averageRating, 2);
                await _userManager.UpdateAsync(user);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assignment_Example_HU.DTOs;

namespace Assignment_Example_HU.Services.Interfaces
{
    public interface IRatingService
    {
        Task<RatingDto> CreateRatingAsync(Guid userId, CreateRatingDto dto);
        Task<IEnumerable<RatingDto>> GetVenueRatingsAsync(Guid venueId);
        Task<IEnumerable<RatingDto>> GetCourtRatingsAsync(Guid courtId);
        Task<IEnumerable<RatingDto>> GetPlayerRatingsAsync(Guid playerId);
        Task<PlayerProfileDto> GetPlayerProfileAsync(Guid userId);
        Task<VenueRatingSummaryDto> GetVenueRatingSummaryAsync(Guid venueId);
        Task<CourtRatingSummaryDto> GetCourtRatingSummaryAsync(Guid courtId);
        Task UpdatePlayerAggregatedRatingAsync(Guid playerId);
    }
}

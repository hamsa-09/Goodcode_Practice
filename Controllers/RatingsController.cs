using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Assignment_Example_HU.DTOs;
using Assignment_Example_HU.Services.Interfaces;

namespace Assignment_Example_HU.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RatingsController : ControllerBase
    {
        private readonly IRatingService _ratingService;

        public RatingsController(IRatingService ratingService)
        {
            _ratingService = ratingService;
        }

        /// <summary>
        /// Create a rating (venue, court, or player).
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<RatingDto>> CreateRating([FromBody] CreateRatingDto dto)
        {
            var userId = GetCurrentUserId();
            var rating = await _ratingService.CreateRatingAsync(userId, dto);
            return Ok(rating);
        }

        /// <summary>
        /// Get ratings for a venue.
        /// </summary>
        [HttpGet("venue/{venueId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<RatingDto>>> GetVenueRatings(Guid venueId)
        {
            var ratings = await _ratingService.GetVenueRatingsAsync(venueId);
            return Ok(ratings);
        }

        /// <summary>
        /// Get ratings for a court.
        /// </summary>
        [HttpGet("court/{courtId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<RatingDto>>> GetCourtRatings(Guid courtId)
        {
            var ratings = await _ratingService.GetCourtRatingsAsync(courtId);
            return Ok(ratings);
        }

        /// <summary>
        /// Get ratings for a player.
        /// </summary>
        [HttpGet("player/{playerId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<RatingDto>>> GetPlayerRatings(Guid playerId)
        {
            var ratings = await _ratingService.GetPlayerRatingsAsync(playerId);
            return Ok(ratings);
        }

        /// <summary>
        /// Get player profile with aggregated rating and recent reviews.
        /// </summary>
        [HttpGet("player/{playerId}/profile")]
        [AllowAnonymous]
        public async Task<ActionResult<PlayerProfileDto>> GetPlayerProfile(Guid playerId)
        {
            var profile = await _ratingService.GetPlayerProfileAsync(playerId);
            return Ok(profile);
        }

        /// <summary>
        /// Get venue rating summary (average and total).
        /// </summary>
        [HttpGet("venue/{venueId}/summary")]
        [AllowAnonymous]
        public async Task<ActionResult<VenueRatingSummaryDto>> GetVenueRatingSummary(Guid venueId)
        {
            var summary = await _ratingService.GetVenueRatingSummaryAsync(venueId);
            return Ok(summary);
        }

        /// <summary>
        /// Get court rating summary (average and total).
        /// </summary>
        [HttpGet("court/{courtId}/summary")]
        [AllowAnonymous]
        public async Task<ActionResult<CourtRatingSummaryDto>> GetCourtRatingSummary(Guid courtId)
        {
            var summary = await _ratingService.GetCourtRatingSummaryAsync(courtId);
            return Ok(summary);
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("Invalid user token.");
            }
            return userId;
        }
    }
}

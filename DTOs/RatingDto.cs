using System;
using System.Collections.Generic;
using Assignment_Example_HU.Enums;

namespace Assignment_Example_HU.DTOs
{
    public class RatingDto
    {
        public Guid Id { get; set; }
        public RatingType Type { get; set; }
        public Guid RatedById { get; set; }
        public string RatedByName { get; set; } = default!;
        public Guid? VenueId { get; set; }
        public string? VenueName { get; set; }
        public Guid? CourtId { get; set; }
        public string? CourtName { get; set; }
        public Guid? PlayerId { get; set; }
        public string? PlayerName { get; set; }
        public Guid? GameId { get; set; }
        public int Score { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateRatingDto
    {
        public RatingType Type { get; set; }
        public Guid? VenueId { get; set; }
        public Guid? CourtId { get; set; }
        public Guid? PlayerId { get; set; }
        public Guid? GameId { get; set; } // Required for player ratings
        public int Score { get; set; } // 1-5
        public string? Comment { get; set; }
    }

    public class PlayerProfileDto
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = default!;
        public decimal? AggregatedRating { get; set; }
        public int GamesPlayed { get; set; }
        public string? PreferredSports { get; set; }
        public List<RecentReviewDto> RecentReviews { get; set; } = new List<RecentReviewDto>();
    }

    public class RecentReviewDto
    {
        public Guid RatingId { get; set; }
        public int Score { get; set; }
        public string? Comment { get; set; }
        public string RatedByName { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
    }

    public class VenueRatingSummaryDto
    {
        public Guid VenueId { get; set; }
        public string VenueName { get; set; } = default!;
        public decimal AverageRating { get; set; }
        public int TotalRatings { get; set; }
    }

    public class CourtRatingSummaryDto
    {
        public Guid CourtId { get; set; }
        public string CourtName { get; set; } = default!;
        public decimal AverageRating { get; set; }
        public int TotalRatings { get; set; }
    }
}

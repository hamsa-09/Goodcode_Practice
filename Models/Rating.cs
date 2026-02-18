using System;
using Assignment_Example_HU.Enums;

namespace Assignment_Example_HU.Models
{
    public class Rating
    {
        public Guid Id { get; set; }
        public RatingType Type { get; set; }
        public Guid RatedById { get; set; } // User who gave the rating
        public Guid? VenueId { get; set; }
        public Guid? CourtId { get; set; }
        public Guid? PlayerId { get; set; } // For player ratings
        public Guid? GameId { get; set; } // Required for player ratings, optional for venue/court
        public int Score { get; set; } // 1-5 stars
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public User RatedBy { get; set; } = default!;
        public Venue? Venue { get; set; }
        public Court? Court { get; set; }
        public User? Player { get; set; }
        public Game? Game { get; set; }
    }
}

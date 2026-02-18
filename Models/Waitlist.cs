using System;

namespace Assignment_Example_HU.Models
{
    public class Waitlist
    {
        public Guid Id { get; set; }
        public Guid GameId { get; set; }
        public Guid UserId { get; set; }
        public decimal PlayerRating { get; set; } // Aggregated rating for sorting
        public int Priority { get; set; } // Calculated based on rating and join time
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Game Game { get; set; } = default!;
        public User User { get; set; } = default!;
    }
}

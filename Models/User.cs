using System;
using System.Collections.Generic;
using Assignment_Example_HU.Enums;

namespace Assignment_Example_HU.Models
{
    public class User : Microsoft.AspNetCore.Identity.IdentityUser<Guid>
    {
        // Identity provides: Id, UserName, Email, PasswordHash, SecurityStamp, etc.

        public Role Role { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoginAt { get; set; }

        // Player profile fields (calculated/aggregated)
        public decimal? AggregatedRating { get; set; } // Average rating from other players
        public int GamesPlayed { get; set; } = 0;
        public string? PreferredSports { get; set; } // Comma-separated sport types

        // Navigation properties
        public ICollection<Venue> OwnedVenues { get; set; } = new List<Venue>();
        public ICollection<Slot> BookedSlots { get; set; } = new List<Slot>();
        public ICollection<Game> CreatedGames { get; set; } = new List<Game>();
        public ICollection<GamePlayer> GamePlayers { get; set; } = new List<GamePlayer>();
        public Wallet? Wallet { get; set; }
        public ICollection<Waitlist> Waitlists { get; set; } = new List<Waitlist>();
        public ICollection<Rating> RatingsGiven { get; set; } = new List<Rating>();
        public ICollection<Rating> RatingsReceived { get; set; } = new List<Rating>(); // As a player
    }
}

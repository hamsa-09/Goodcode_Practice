using System;

namespace Assignment_Example_HU.Models
{
    public class GamePlayer
    {
        public Guid Id { get; set; }
        public Guid GameId { get; set; }
        public Guid UserId { get; set; }
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Game Game { get; set; } = default!;
        public User User { get; set; } = default!;
    }
}

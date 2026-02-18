using System;
using System.Collections.Generic;
using Assignment_Example_HU.Enums;

namespace Assignment_Example_HU.Models
{
    public class Game
    {
        public Guid Id { get; set; }
        public Guid SlotId { get; set; }
        public GameType Type { get; set; }
        public int MinPlayers { get; set; }
        public int MaxPlayers { get; set; }
        public GameStatus Status { get; set; } = GameStatus.Pending;
        public Guid CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CancelledAt { get; set; }

        // Navigation properties
        public Slot Slot { get; set; } = default!;
        public User CreatedByUser { get; set; } = default!;
        public ICollection<GamePlayer> Players { get; set; } = new List<GamePlayer>();
        public ICollection<Waitlist> Waitlist { get; set; } = new List<Waitlist>();
        public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
    }
}

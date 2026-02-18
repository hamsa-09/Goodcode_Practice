using System;
using System.Collections.Generic;
using Assignment_Example_HU.Enums;

namespace Assignment_Example_HU.Models
{
    public class Slot
    {
        public Guid Id { get; set; }
        public Guid CourtId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal Price { get; set; } // Final calculated price
        public SlotStatus Status { get; set; } = SlotStatus.Available;
        public Guid? BookedByUserId { get; set; }
        public DateTime? BookedAt { get; set; }
        public DateTime? LockedUntil { get; set; } // For price lock mechanism

        // Navigation properties
        public Court Court { get; set; } = default!;
        public User? BookedByUser { get; set; }
        public Game? Game { get; set; }
    }
}

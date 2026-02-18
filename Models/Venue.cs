using System;
using System.Collections.Generic;
using Assignment_Example_HU.Enums;

namespace Assignment_Example_HU.Models
{
    public class Venue
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string Address { get; set; } = default!;
        public string SportsSupported { get; set; } = default!; // Comma-separated sport types
        public Guid OwnerId { get; set; }
        public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public User Owner { get; set; } = default!;
        public ICollection<Court> Courts { get; set; } = new List<Court>();
        public ICollection<Discount> Discounts { get; set; } = new List<Discount>();
        public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
    }
}

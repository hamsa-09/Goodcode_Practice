using System;
using System.Collections.Generic;
using Assignment_Example_HU.Enums;

namespace Assignment_Example_HU.Models
{
    public class Court
    {
        public Guid Id { get; set; }
        public Guid VenueId { get; set; }
        public string Name { get; set; } = default!;
        public SportType SportType { get; set; }
        public int SlotDurationMinutes { get; set; } = 60; // Default 1 hour
        public decimal BasePrice { get; set; }
        public string OperatingHours { get; set; } = default!; // JSON or format like "09:00-22:00"
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public Venue Venue { get; set; } = default!;
        public ICollection<Slot> Slots { get; set; } = new List<Slot>();
        public ICollection<Discount> Discounts { get; set; } = new List<Discount>();
        public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
    }
}

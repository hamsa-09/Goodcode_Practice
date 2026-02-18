using System;
using Assignment_Example_HU.Enums;

namespace Assignment_Example_HU.Models
{
    public class Discount
    {
        public Guid Id { get; set; }
        public DiscountScope Scope { get; set; }
        public Guid? VenueId { get; set; }
        public Guid? CourtId { get; set; }
        public decimal PercentOff { get; set; } // e.g., 10.0 means 10% off
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public Venue? Venue { get; set; }
        public Court? Court { get; set; }
    }
}

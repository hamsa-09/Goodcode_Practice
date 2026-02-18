using System;
using System.Collections.Generic;

namespace Assignment_Example_HU.DTOs
{
    public class AdminStatsDto
    {
        public int TotalUsers { get; set; }
        public int TotalVenues { get; set; }
        public int TotalCourts { get; set; }
        public int TotalBookings { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<VenuePopularityDto> PopularVenues { get; set; } = new();
    }

    public class VenuePopularityDto
    {
        public Guid VenueId { get; set; }
        public string VenueName { get; set; } = string.Empty;
        public int BookingCount { get; set; }
    }
}

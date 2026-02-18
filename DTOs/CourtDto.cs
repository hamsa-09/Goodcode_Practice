using System;
using Assignment_Example_HU.Enums;

namespace Assignment_Example_HU.DTOs
{
    public class CourtDto
    {
        public Guid Id { get; set; }
        public Guid VenueId { get; set; }
        public string VenueName { get; set; } = default!;
        public string Name { get; set; } = default!;
        public SportType SportType { get; set; }
        public int SlotDurationMinutes { get; set; }
        public decimal BasePrice { get; set; }
        public string OperatingHours { get; set; } = default!;
        public bool IsActive { get; set; }
    }

    public class CreateCourtDto
    {
        public Guid VenueId { get; set; }
        public string Name { get; set; } = default!;
        public SportType SportType { get; set; }
        public int SlotDurationMinutes { get; set; } = 60;
        public decimal BasePrice { get; set; }
        public string OperatingHours { get; set; } = default!;
    }

    public class UpdateCourtDto
    {
        public string? Name { get; set; }
        public SportType? SportType { get; set; }
        public int? SlotDurationMinutes { get; set; }
        public decimal? BasePrice { get; set; }
        public string? OperatingHours { get; set; }
        public bool? IsActive { get; set; }
    }
}

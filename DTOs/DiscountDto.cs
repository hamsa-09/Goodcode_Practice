using System;
using Assignment_Example_HU.Enums;

namespace Assignment_Example_HU.DTOs
{
    public class DiscountDto
    {
        public Guid Id { get; set; }
        public DiscountScope Scope { get; set; }
        public Guid? VenueId { get; set; }
        public string? VenueName { get; set; }
        public Guid? CourtId { get; set; }
        public string? CourtName { get; set; }
        public decimal PercentOff { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateDiscountDto
    {
        public DiscountScope Scope { get; set; }
        public Guid? VenueId { get; set; }
        public Guid? CourtId { get; set; }
        public decimal PercentOff { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
    }

    public class UpdateDiscountDto
    {
        public decimal? PercentOff { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public bool? IsActive { get; set; }
    }
}

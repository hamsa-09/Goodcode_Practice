using System;
using Assignment_Example_HU.Enums;

namespace Assignment_Example_HU.DTOs
{
    public class SlotDto
    {
        public Guid Id { get; set; }
        public Guid CourtId { get; set; }
        public string CourtName { get; set; } = default!;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal BasePrice { get; set; }
        public decimal FinalPrice { get; set; }
        public SlotStatus Status { get; set; }
        public Guid? BookedByUserId { get; set; }
        public DateTime? LockedUntil { get; set; }
        public bool IsPriceLocked { get; set; }
    }

    public class AvailableSlotDto
    {
        public Guid Id { get; set; }
        public Guid CourtId { get; set; }
        public string CourtName { get; set; } = default!;
        public string VenueName { get; set; } = default!;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal BasePrice { get; set; }
        public decimal FinalPrice { get; set; }
        public int ViewersCount { get; set; }
        public bool IsPriceLocked { get; set; }
        public DateTime? PriceLockExpiresAt { get; set; }
    }

    public class BookSlotDto
    {
        public Guid SlotId { get; set; }
    }

    public class BookSlotResponseDto
    {
        public Guid SlotId { get; set; }
        public Guid CourtId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal FinalPrice { get; set; }
        public SlotStatus Status { get; set; }
        public DateTime? LockedUntil { get; set; }
    }

    public class LockSlotDto
    {
        public Guid SlotId { get; set; }
    }

    public class PriceCalculationDto
    {
        public decimal BasePrice { get; set; }
        public decimal DemandMultiplier { get; set; }
        public decimal TimeMultiplier { get; set; }
        public decimal HistoricalMultiplier { get; set; }
        public decimal DiscountFactor { get; set; }
        public decimal FinalPrice { get; set; }
    }
}

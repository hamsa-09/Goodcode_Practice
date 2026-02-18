using System;
using Assignment_Example_HU.Enums;

namespace Assignment_Example_HU.Models
{
    public class Refund
    {
        public Guid Id { get; set; }
        public Guid TransactionId { get; set; }
        public Guid SlotId { get; set; }
        public Guid UserId { get; set; }
        public decimal OriginalAmount { get; set; }
        public decimal RefundAmount { get; set; }
        public decimal RefundPercentage { get; set; } // e.g., 100, 50, 0
        public RefundStatus Status { get; set; } = RefundStatus.Pending;
        public string Reason { get; set; } = default!; // e.g., "Cancelled >24h", "Venue unavailable"
        public string? ReferenceId { get; set; } // For idempotency
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedAt { get; set; }

        // Navigation properties
        public Transaction Transaction { get; set; } = default!;
        public Slot Slot { get; set; } = default!;
        public User User { get; set; } = default!;
    }
}

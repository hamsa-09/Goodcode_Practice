using System;
using Assignment_Example_HU.Enums;

namespace Assignment_Example_HU.Models
{
    public class Transaction
    {
        public Guid Id { get; set; }
        public Guid WalletId { get; set; }
        public Guid UserId { get; set; }
        public TransactionType Type { get; set; }
        public decimal Amount { get; set; }
        public decimal BalanceBefore { get; set; }
        public decimal BalanceAfter { get; set; }
        public TransactionStatus Status { get; set; } = TransactionStatus.Pending;
        public string? Description { get; set; }
        public string? ReferenceId { get; set; } // For idempotency (e.g., payment gateway transaction ID)
        public Guid? RelatedSlotId { get; set; } // For booking payments
        public Guid? RelatedGameId { get; set; } // For game-related transactions
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }

        // Navigation properties
        public Wallet Wallet { get; set; } = default!;
        public User User { get; set; } = default!;
        public Slot? RelatedSlot { get; set; }
        public Game? RelatedGame { get; set; }
    }
}

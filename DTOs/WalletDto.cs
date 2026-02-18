using System;
using System.Collections.Generic;
using Assignment_Example_HU.Enums;

namespace Assignment_Example_HU.DTOs
{
    public class WalletDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = default!;
        public decimal Balance { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AddFundsDto
    {
        public decimal Amount { get; set; }
        public string? ReferenceId { get; set; } // For idempotency
        public string? Description { get; set; }
    }

    public class TransactionDto
    {
        public Guid Id { get; set; }
        public Guid WalletId { get; set; }
        public TransactionType Type { get; set; }
        public decimal Amount { get; set; }
        public decimal BalanceBefore { get; set; }
        public decimal BalanceAfter { get; set; }
        public TransactionStatus Status { get; set; }
        public string? Description { get; set; }
        public string? ReferenceId { get; set; }
        public Guid? RelatedSlotId { get; set; }
        public Guid? RelatedGameId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    public class PaymentDto
    {
        public Guid SlotId { get; set; }
        public string? ReferenceId { get; set; } // For idempotency
    }

    public class PaymentResponseDto
    {
        public Guid TransactionId { get; set; }
        public Guid SlotId { get; set; }
        public decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; }
        public TransactionStatus Status { get; set; }
    }

    public class RefundDto
    {
        public Guid Id { get; set; }
        public Guid TransactionId { get; set; }
        public Guid SlotId { get; set; }
        public decimal OriginalAmount { get; set; }
        public decimal RefundAmount { get; set; }
        public decimal RefundPercentage { get; set; }
        public RefundStatus Status { get; set; }
        public string Reason { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }

    public class RequestRefundDto
    {
        public Guid SlotId { get; set; }
        public string? Reason { get; set; }
    }
}

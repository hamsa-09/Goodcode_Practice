using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Assignment_Example_HU.Data;
using Assignment_Example_HU.DTOs;
using Assignment_Example_HU.Enums;
using Assignment_Example_HU.Models;
using Assignment_Example_HU.Repositories.Interfaces;
using Assignment_Example_HU.Services.Interfaces;

namespace Assignment_Example_HU.Services
{
    public class RefundService : IRefundService
    {
        private readonly IRefundRepository _refundRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly ISlotRepository _slotRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;

        public RefundService(
            IRefundRepository refundRepository,
            ITransactionRepository transactionRepository,
            ISlotRepository slotRepository,
            IWalletRepository walletRepository,
            AppDbContext dbContext,
            IMapper mapper)
        {
            _refundRepository = refundRepository;
            _transactionRepository = transactionRepository;
            _slotRepository = slotRepository;
            _walletRepository = walletRepository;
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<RefundDto> RequestRefundAsync(Guid userId, RequestRefundDto dto)
        {
            var slot = await _slotRepository.GetByIdAsync(dto.SlotId);
            if (slot == null)
            {
                throw new InvalidOperationException("Slot not found.");
            }

            if (slot.BookedByUserId != userId)
            {
                throw new UnauthorizedAccessException("You can only request refunds for your own bookings.");
            }

            if (slot.Status != SlotStatus.Booked)
            {
                throw new InvalidOperationException("Slot must be booked to request a refund.");
            }

            // Find the payment transaction
            var transactions = await _transactionRepository.GetByUserIdAsync(userId);
            var paymentTransaction = transactions
                .FirstOrDefault(t => t.RelatedSlotId == dto.SlotId &&
                                    t.Type == TransactionType.Debit &&
                                    t.Status == TransactionStatus.Completed);

            if (paymentTransaction == null)
            {
                throw new InvalidOperationException("Payment transaction not found for this slot.");
            }

            // Check if refund already exists
            var existingRefund = await _refundRepository.GetByReferenceIdAsync(paymentTransaction.Id.ToString());
            if (existingRefund != null)
            {
                return _mapper.Map<RefundDto>(existingRefund);
            }

            // Calculate refund amount
            var refundAmount = await CalculateRefundAmountAsync(
                slot.StartTime,
                paymentTransaction.Amount,
                dto.Reason);

            var refund = new Refund
            {
                Id = Guid.NewGuid(),
                TransactionId = paymentTransaction.Id,
                SlotId = dto.SlotId,
                UserId = userId,
                OriginalAmount = paymentTransaction.Amount,
                RefundAmount = refundAmount,
                RefundPercentage = refundAmount > 0 ? (refundAmount / paymentTransaction.Amount) * 100 : 0,
                Status = RefundStatus.Pending,
                Reason = dto.Reason ?? "User requested refund",
                ReferenceId = paymentTransaction.Id.ToString()
            };

            await _refundRepository.AddAsync(refund);
            await _refundRepository.SaveChangesAsync();

            return _mapper.Map<RefundDto>(refund);
        }

        public async Task ProcessPendingRefundsAsync()
        {
            var pendingRefunds = await _refundRepository.GetPendingRefundsAsync();

            foreach (var refund in pendingRefunds)
            {
                try
                {
                    refund.Status = RefundStatus.Processing;
                    await _refundRepository.UpdateAsync(refund);

                    // Get transaction and wallet
                    var transaction = await _transactionRepository.GetByIdAsync(refund.TransactionId);
                    if (transaction == null || transaction.Status != TransactionStatus.Completed)
                    {
                        refund.Status = RefundStatus.Failed;
                        refund.Reason = "Original transaction not found or not completed";
                        await _refundRepository.UpdateAsync(refund);
                        continue;
                    }

                    var wallet = await _walletRepository.GetByUserIdAsync(refund.UserId);
                    if (wallet == null)
                    {
                        refund.Status = RefundStatus.Failed;
                        refund.Reason = "Wallet not found";
                        await _refundRepository.UpdateAsync(refund);
                        continue;
                    }

                    // ACID transaction: Credit wallet + Update refund status
                    using var dbTransaction = await _dbContext.Database.BeginTransactionAsync();
                    try
                    {
                        // Lock wallet
                        var lockedWallet = await _dbContext.Wallets
                            .FirstOrDefaultAsync(w => w.Id == wallet.Id);

                        if (lockedWallet == null)
                        {
                            throw new InvalidOperationException("Wallet not found.");
                        }

                        var balanceBefore = lockedWallet.Balance;
                        var balanceAfter = balanceBefore + refund.RefundAmount;

                        // Create credit transaction
                        var creditTransaction = new Transaction
                        {
                            Id = Guid.NewGuid(),
                            WalletId = lockedWallet.Id,
                            UserId = refund.UserId,
                            Type = TransactionType.Credit,
                            Amount = refund.RefundAmount,
                            BalanceBefore = balanceBefore,
                            BalanceAfter = balanceAfter,
                            Status = TransactionStatus.Completed,
                            Description = $"Refund: {refund.Reason}",
                            ReferenceId = refund.Id.ToString(),
                            RelatedSlotId = refund.SlotId,
                            CompletedAt = DateTime.UtcNow
                        };

                        lockedWallet.Balance = balanceAfter;
                        lockedWallet.UpdatedAt = DateTime.UtcNow;

                        refund.Status = RefundStatus.Completed;
                        refund.ProcessedAt = DateTime.UtcNow;

                        // Cancel slot
                        var slot = await _slotRepository.GetByIdAsync(refund.SlotId);
                        if (slot != null)
                        {
                            slot.Status = SlotStatus.Cancelled;
                            slot.BookedByUserId = null;
                            slot.BookedAt = null;
                            await _slotRepository.UpdateAsync(slot);
                        }

                        await _transactionRepository.AddAsync(creditTransaction);
                        await _walletRepository.UpdateAsync(lockedWallet);
                        await _refundRepository.UpdateAsync(refund);
                        await _dbContext.SaveChangesAsync();
                        await dbTransaction.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        await dbTransaction.RollbackAsync();
                        refund.Status = RefundStatus.Failed;
                        refund.Reason = $"Processing failed: {ex.Message}";
                        await _refundRepository.UpdateAsync(refund);
                        await _refundRepository.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    refund.Status = RefundStatus.Failed;
                    refund.Reason = $"Error: {ex.Message}";
                    await _refundRepository.UpdateAsync(refund);
                    await _refundRepository.SaveChangesAsync();
                }
            }
        }

        public async Task<IEnumerable<RefundDto>> GetUserRefundsAsync(Guid userId)
        {
            var refunds = await _refundRepository.GetByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<RefundDto>>(refunds);
        }

        public async Task<decimal> CalculateRefundAmountAsync(DateTime slotStartTime, decimal originalAmount, string? reason = null)
        {
            // Venue unavailable = full refund
            if (!string.IsNullOrEmpty(reason) && reason.Contains("unavailable", StringComparison.OrdinalIgnoreCase))
            {
                return originalAmount;
            }

            var timeUntilStart = slotStartTime - DateTime.UtcNow;
            var hoursUntilStart = timeUntilStart.TotalHours;

            return hoursUntilStart switch
            {
                > 24 => originalAmount,           // 100% refund
                >= 6 and <= 24 => originalAmount * 0.5m,  // 50% refund
                < 6 => 0,                         // 0% refund
                _ => 0
            };
        }
    }
}

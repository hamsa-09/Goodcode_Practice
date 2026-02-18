using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Assignment_Example_HU.Data;
using Assignment_Example_HU.DTOs;
using Assignment_Example_HU.Enums;
using Assignment_Example_HU.Models;
using Assignment_Example_HU.Repositories.Interfaces;
using Assignment_Example_HU.Services.Interfaces;
using AutoMapper;
using System.Collections.Generic;

namespace Assignment_Example_HU.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IWalletService _walletService;
        private readonly ISlotRepository _slotRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;

        public PaymentService(
            IWalletService walletService,
            ISlotRepository slotRepository,
            ITransactionRepository transactionRepository,
            IWalletRepository walletRepository,
            AppDbContext dbContext,
            IMapper mapper)
        {
            _walletService = walletService;
            _slotRepository = slotRepository;
            _transactionRepository = transactionRepository;
            _walletRepository = walletRepository;
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<PaymentResponseDto> ProcessPaymentAsync(Guid userId, PaymentDto dto)
        {
            // Check for idempotency
            if (!string.IsNullOrEmpty(dto.ReferenceId))
            {
                var existingTransaction = await _transactionRepository.GetByReferenceIdAsync(dto.ReferenceId);
                if (existingTransaction != null && existingTransaction.Status == TransactionStatus.Completed)
                {
                    return new PaymentResponseDto
                    {
                        TransactionId = existingTransaction.Id,
                        SlotId = existingTransaction.RelatedSlotId ?? dto.SlotId,
                        Amount = existingTransaction.Amount,
                        BalanceAfter = existingTransaction.BalanceAfter,
                        Status = existingTransaction.Status
                    };
                }
            }

            // Get slot
            var slot = await _slotRepository.GetByIdAsync(dto.SlotId);
            if (slot == null)
            {
                throw new InvalidOperationException("Slot not found.");
            }

            if (slot.Status != SlotStatus.Locked || slot.BookedByUserId != userId)
            {
                throw new InvalidOperationException("Slot must be locked by you before payment.");
            }

            // Ensure wallet exists
            var wallet = await _walletRepository.GetByUserIdAsync(userId);
            if (wallet == null)
            {
                await _walletService.CreateWalletAsync(userId);
                wallet = await _walletRepository.GetByUserIdAsync(userId);
            }

            var paymentAmount = slot.Price;

            // ACID transaction: Debit wallet + Confirm slot booking
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                // Lock wallet row to prevent concurrent updates
                var lockedWallet = await _dbContext.Wallets
                    .FirstOrDefaultAsync(w => w.Id == wallet.Id);

                if (lockedWallet == null)
                {
                    throw new InvalidOperationException("Wallet not found.");
                }

                // Check balance
                if (lockedWallet.Balance < paymentAmount)
                {
                    throw new InvalidOperationException($"Insufficient balance. Required: {paymentAmount}, Available: {lockedWallet.Balance}");
                }

                var balanceBefore = lockedWallet.Balance;
                var balanceAfter = balanceBefore - paymentAmount;

                // Create debit transaction
                var dbTransaction = new Transaction
                {
                    Id = Guid.NewGuid(),
                    WalletId = lockedWallet.Id,
                    UserId = userId,
                    Type = TransactionType.Debit,
                    Amount = paymentAmount,
                    BalanceBefore = balanceBefore,
                    BalanceAfter = balanceAfter,
                    Status = TransactionStatus.Completed,
                    Description = $"Payment for slot booking",
                    ReferenceId = dto.ReferenceId,
                    RelatedSlotId = dto.SlotId,
                    CompletedAt = DateTime.UtcNow
                };

                // Update wallet
                lockedWallet.Balance = balanceAfter;
                lockedWallet.UpdatedAt = DateTime.UtcNow;

                // Confirm slot booking
                slot.Status = SlotStatus.Booked;
                slot.LockedUntil = null;

                await _transactionRepository.AddAsync(dbTransaction);
                await _walletRepository.UpdateAsync(lockedWallet);
                await _slotRepository.UpdateAsync(slot);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                var response = _mapper.Map<PaymentResponseDto>(dbTransaction);
                response.SlotId = dto.SlotId;
                return response;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}

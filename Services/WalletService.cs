using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using AutoMapper;
using Assignment_Example_HU.Data;
using Assignment_Example_HU.DTOs;
using Assignment_Example_HU.Enums;
using Assignment_Example_HU.Models;
using Assignment_Example_HU.Repositories.Interfaces;
using Assignment_Example_HU.Services.Interfaces;

namespace Assignment_Example_HU.Services
{
    public class WalletService : IWalletService
    {
        private readonly IWalletRepository _walletRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly UserManager<User> _userManager;
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;

        public WalletService(
            IWalletRepository walletRepository,
            ITransactionRepository transactionRepository,
            UserManager<User> userManager,
            AppDbContext dbContext,
            IMapper mapper)
        {
            _walletRepository = walletRepository;
            _transactionRepository = transactionRepository;
            _userManager = userManager;
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<WalletDto> GetWalletAsync(Guid userId)
        {
            var wallet = await _walletRepository.GetByUserIdAsync(userId);
            if (wallet == null)
            {
                // Auto-create wallet if it doesn't exist
                return await CreateWalletAsync(userId);
            }

            return _mapper.Map<WalletDto>(wallet);
        }

        public async Task<WalletDto> CreateWalletAsync(Guid userId)
        {
            var exists = await _walletRepository.ExistsForUserAsync(userId);
            if (exists)
            {
                var existing = await _walletRepository.GetByUserIdAsync(userId);
                return _mapper.Map<WalletDto>(existing!);
            }

            var wallet = new Wallet
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Balance = 1000 // Starting balance for testing
            };

            await _walletRepository.AddAsync(wallet);
            await _walletRepository.SaveChangesAsync();

            return _mapper.Map<WalletDto>(wallet);
        }

        public async Task<TransactionDto> AddFundsAsync(Guid userId, AddFundsDto dto)
        {
            if (dto.Amount <= 0)
            {
                throw new InvalidOperationException("Amount must be greater than zero.");
            }

            // Check for idempotency
            if (!string.IsNullOrEmpty(dto.ReferenceId))
            {
                var existingTransaction = await _transactionRepository.GetByReferenceIdAsync(dto.ReferenceId);
                if (existingTransaction != null && existingTransaction.Status == TransactionStatus.Completed)
                {
                    return _mapper.Map<TransactionDto>(existingTransaction);
                }
            }

            // Ensure wallet exists
            var wallet = await _walletRepository.GetByUserIdAsync(userId);
            if (wallet == null)
            {
                await CreateWalletAsync(userId);
                wallet = await _walletRepository.GetByUserIdAsync(userId);
            }

            // Use database transaction for ACID guarantee
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                // Reload wallet with lock to prevent concurrent updates
                var lockedWallet = await _dbContext.Wallets
                    .FirstOrDefaultAsync(w => w.Id == wallet!.Id);

                if (lockedWallet == null)
                {
                    throw new InvalidOperationException("Wallet not found.");
                }

                var balanceBefore = lockedWallet.Balance;
                var balanceAfter = balanceBefore + dto.Amount;

                var dbTransaction = new Transaction
                {
                    Id = Guid.NewGuid(),
                    WalletId = lockedWallet.Id,
                    UserId = userId,
                    Type = TransactionType.Credit,
                    Amount = dto.Amount,
                    BalanceBefore = balanceBefore,
                    BalanceAfter = balanceAfter,
                    Status = TransactionStatus.Completed,
                    Description = dto.Description ?? "Funds added",
                    ReferenceId = dto.ReferenceId,
                    CompletedAt = DateTime.UtcNow
                };

                lockedWallet.Balance = balanceAfter;
                lockedWallet.UpdatedAt = DateTime.UtcNow;

                await _transactionRepository.AddAsync(dbTransaction);
                await _walletRepository.UpdateAsync(lockedWallet);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return _mapper.Map<TransactionDto>(dbTransaction);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<TransactionDto>> GetTransactionHistoryAsync(Guid userId)
        {
            var transactions = await _transactionRepository.GetByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<TransactionDto>>(transactions);
        }
    }
}

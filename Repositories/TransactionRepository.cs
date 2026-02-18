using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Assignment_Example_HU.Data;
using Assignment_Example_HU.Models;
using Assignment_Example_HU.Repositories.Interfaces;

namespace Assignment_Example_HU.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly AppDbContext _dbContext;

        public TransactionRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Transaction?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Transactions
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Transaction?> GetByReferenceIdAsync(string referenceId)
        {
            if (string.IsNullOrEmpty(referenceId))
                return null;

            return await _dbContext.Transactions
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.ReferenceId == referenceId);
        }

        public async Task<IEnumerable<Transaction>> GetByWalletIdAsync(Guid walletId)
        {
            return await _dbContext.Transactions
                .AsNoTracking()
                .Where(t => t.WalletId == walletId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetByUserIdAsync(Guid userId)
        {
            return await _dbContext.Transactions
                .AsNoTracking()
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetAllAsync()
        {
            return await _dbContext.Transactions
                .AsNoTracking()
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task AddAsync(Transaction transaction)
        {
            await _dbContext.Transactions.AddAsync(transaction);
        }

        public async Task UpdateAsync(Transaction transaction)
        {
            _dbContext.Transactions.Update(transaction);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}

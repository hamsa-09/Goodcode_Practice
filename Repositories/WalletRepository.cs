using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Assignment_Example_HU.Data;
using Assignment_Example_HU.Models;
using Assignment_Example_HU.Repositories.Interfaces;

namespace Assignment_Example_HU.Repositories
{
    public class WalletRepository : IWalletRepository
    {
        private readonly AppDbContext _dbContext;

        public WalletRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Wallet?> GetByUserIdAsync(Guid userId)
        {
            return await _dbContext.Wallets
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.UserId == userId);
        }

        public async Task<Wallet?> GetByUserIdWithTransactionsAsync(Guid userId)
        {
            return await _dbContext.Wallets
                .Include(w => w.Transactions)
                .FirstOrDefaultAsync(w => w.UserId == userId);
        }

        public async Task AddAsync(Wallet wallet)
        {
            await _dbContext.Wallets.AddAsync(wallet);
        }

        public async Task UpdateAsync(Wallet wallet)
        {
            _dbContext.Wallets.Update(wallet);
            await Task.CompletedTask;
        }

        public async Task<bool> ExistsForUserAsync(Guid userId)
        {
            return await _dbContext.Wallets.AnyAsync(w => w.UserId == userId);
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}

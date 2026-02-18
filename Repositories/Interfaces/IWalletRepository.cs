using System;
using System.Threading.Tasks;
using Assignment_Example_HU.Models;

namespace Assignment_Example_HU.Repositories.Interfaces
{
    public interface IWalletRepository
    {
        Task<Wallet?> GetByUserIdAsync(Guid userId);
        Task<Wallet?> GetByUserIdWithTransactionsAsync(Guid userId);
        Task AddAsync(Wallet wallet);
        Task UpdateAsync(Wallet wallet);
        Task<bool> ExistsForUserAsync(Guid userId);
        Task SaveChangesAsync();
    }
}

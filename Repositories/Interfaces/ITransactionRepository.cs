using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assignment_Example_HU.Enums;
using Assignment_Example_HU.Models;

namespace Assignment_Example_HU.Repositories.Interfaces
{
    public interface ITransactionRepository
    {
        Task<Transaction?> GetByIdAsync(Guid id);
        Task<Transaction?> GetByReferenceIdAsync(string referenceId);
        Task<IEnumerable<Transaction>> GetByWalletIdAsync(Guid walletId);
        Task<IEnumerable<Transaction>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<Transaction>> GetAllAsync();
        Task AddAsync(Transaction transaction);
        Task UpdateAsync(Transaction transaction);
        Task SaveChangesAsync();
    }
}

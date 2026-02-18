using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assignment_Example_HU.Enums;
using Assignment_Example_HU.Models;

namespace Assignment_Example_HU.Repositories.Interfaces
{
    public interface IRefundRepository
    {
        Task<Refund?> GetByIdAsync(Guid id);
        Task<Refund?> GetByReferenceIdAsync(string referenceId);
        Task<IEnumerable<Refund>> GetPendingRefundsAsync();
        Task<IEnumerable<Refund>> GetByUserIdAsync(Guid userId);
        Task AddAsync(Refund refund);
        Task UpdateAsync(Refund refund);
        Task SaveChangesAsync();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Assignment_Example_HU.Data;
using Assignment_Example_HU.Enums;
using Assignment_Example_HU.Models;
using Assignment_Example_HU.Repositories.Interfaces;

namespace Assignment_Example_HU.Repositories
{
    public class RefundRepository : IRefundRepository
    {
        private readonly AppDbContext _dbContext;

        public RefundRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Refund?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Refunds
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<Refund?> GetByReferenceIdAsync(string referenceId)
        {
            if (string.IsNullOrEmpty(referenceId))
                return null;

            return await _dbContext.Refunds
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.ReferenceId == referenceId);
        }

        public async Task<IEnumerable<Refund>> GetPendingRefundsAsync()
        {
            return await _dbContext.Refunds
                .Where(r => r.Status == RefundStatus.Pending || r.Status == RefundStatus.Processing)
                .OrderBy(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Refund>> GetByUserIdAsync(Guid userId)
        {
            return await _dbContext.Refunds
                .AsNoTracking()
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task AddAsync(Refund refund)
        {
            await _dbContext.Refunds.AddAsync(refund);
        }

        public async Task UpdateAsync(Refund refund)
        {
            _dbContext.Refunds.Update(refund);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}

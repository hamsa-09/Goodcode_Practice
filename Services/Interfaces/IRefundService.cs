using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assignment_Example_HU.DTOs;

namespace Assignment_Example_HU.Services.Interfaces
{
    public interface IRefundService
    {
        Task<RefundDto> RequestRefundAsync(Guid userId, RequestRefundDto dto);
        Task ProcessPendingRefundsAsync();
        Task<IEnumerable<RefundDto>> GetUserRefundsAsync(Guid userId);
        Task<decimal> CalculateRefundAmountAsync(DateTime slotStartTime, decimal originalAmount, string? reason = null);
    }
}

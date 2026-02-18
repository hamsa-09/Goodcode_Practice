using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assignment_Example_HU.DTOs;

namespace Assignment_Example_HU.Services.Interfaces
{
    public interface IWalletService
    {
        Task<WalletDto> GetWalletAsync(Guid userId);
        Task<WalletDto> CreateWalletAsync(Guid userId);
        Task<TransactionDto> AddFundsAsync(Guid userId, AddFundsDto dto);
        Task<IEnumerable<TransactionDto>> GetTransactionHistoryAsync(Guid userId);
    }
}

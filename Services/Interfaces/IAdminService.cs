using System.Collections.Generic;
using System.Threading.Tasks;
using Assignment_Example_HU.DTOs;

namespace Assignment_Example_HU.Services.Interfaces
{
    public interface IAdminService
    {
        Task<AdminStatsDto> GetDashboardStatsAsync();
        Task<IEnumerable<TransactionDto>> GetAllTransactionsAsync();
        Task<IEnumerable<VenueDto>> GetPendingVenuesAsync();
    }
}

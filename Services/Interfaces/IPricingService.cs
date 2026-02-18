using System;
using System.Threading.Tasks;
using Assignment_Example_HU.DTOs;

namespace Assignment_Example_HU.Services.Interfaces
{
    public interface IPricingService
    {
        Task<PriceCalculationDto> CalculatePriceAsync(
            Guid slotId,
            decimal basePrice,
            DateTime startTime,
            Guid? courtId = null,
            Guid? venueId = null);

        Task<decimal> GetDemandMultiplierAsync(Guid slotId);
        Task<decimal> GetTimeMultiplierAsync(DateTime startTime);
        Task<decimal> GetHistoricalMultiplierAsync(Guid courtId, DateTime startTime);
        Task<decimal> GetDiscountFactorAsync(Guid? venueId, Guid? courtId, DateTime startTime);
    }
}

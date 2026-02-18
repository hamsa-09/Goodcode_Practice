using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Assignment_Example_HU.Data;
using Assignment_Example_HU.DTOs;
using Assignment_Example_HU.Enums;
using Assignment_Example_HU.Repositories.Interfaces;
using Assignment_Example_HU.Services.Interfaces;

namespace Assignment_Example_HU.Services
{
    public class PricingService : IPricingService
    {
        private readonly IDemandTrackingService _demandTrackingService;
        private readonly IDiscountRepository _discountRepository;
        private readonly ISlotRepository _slotRepository;
        private readonly AppDbContext _dbContext;

        public PricingService(
            IDemandTrackingService demandTrackingService,
            IDiscountRepository discountRepository,
            ISlotRepository slotRepository,
            AppDbContext dbContext)
        {
            _demandTrackingService = demandTrackingService;
            _discountRepository = discountRepository;
            _slotRepository = slotRepository;
            _dbContext = dbContext;
        }

        public async Task<PriceCalculationDto> CalculatePriceAsync(
            Guid slotId,
            decimal basePrice,
            DateTime startTime,
            Guid? courtId = null,
            Guid? venueId = null)
        {
            var demandMultiplier = await GetDemandMultiplierAsync(slotId);
            var timeMultiplier = await GetTimeMultiplierAsync(startTime);
            var historicalMultiplier = courtId.HasValue 
                ? await GetHistoricalMultiplierAsync(courtId.Value, startTime)
                : 1.0m;
            var discountFactor = await GetDiscountFactorAsync(venueId, courtId, startTime);

            var finalPrice = basePrice * demandMultiplier * timeMultiplier * historicalMultiplier * discountFactor;

            return new PriceCalculationDto
            {
                BasePrice = basePrice,
                DemandMultiplier = demandMultiplier,
                TimeMultiplier = timeMultiplier,
                HistoricalMultiplier = historicalMultiplier,
                DiscountFactor = discountFactor,
                FinalPrice = Math.Round(finalPrice, 2)
            };
        }

        public async Task<decimal> GetDemandMultiplierAsync(Guid slotId)
        {
            var viewerCount = await _demandTrackingService.GetViewerCountAsync(slotId);

            return viewerCount switch
            {
                0 => 1.0m,
                >= 2 and <= 5 => 1.2m,
                > 5 => 1.5m,
                _ => 1.0m
            };
        }

        public async Task<decimal> GetTimeMultiplierAsync(DateTime startTime)
        {
            var timeUntilStart = startTime - DateTime.UtcNow;

            return timeUntilStart.TotalHours switch
            {
                > 24 => 1.0m,
                >= 6 and <= 24 => 1.2m,
                < 6 => 1.5m,
                _ => 1.0m
            };
        }

        public async Task<decimal> GetHistoricalMultiplierAsync(Guid courtId, DateTime startTime)
        {
            // Calculate booking rate for this court at similar times (same day of week, similar hour)
            var dayOfWeek = startTime.DayOfWeek;
            var hour = startTime.Hour;

            // Get bookings for this court in the last 30 days at similar times
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            var similarTimeSlots = await _dbContext.Slots
                .Where(s => s.CourtId == courtId &&
                           s.StartTime >= thirtyDaysAgo &&
                           s.StartTime.DayOfWeek == dayOfWeek &&
                           s.StartTime.Hour >= hour - 1 &&
                           s.StartTime.Hour <= hour + 1 &&
                           (s.Status == Enums.SlotStatus.Booked || s.Status == Enums.SlotStatus.Completed))
                .CountAsync();

            // Calculate booking rate (simplified: if > 20 bookings, high; > 10 medium; else low)
            return similarTimeSlots switch
            {
                > 20 => 1.5m,  // High demand
                > 10 => 1.2m,  // Medium demand
                _ => 1.0m      // Low demand
            };
        }

        public async Task<decimal> GetDiscountFactorAsync(Guid? venueId, Guid? courtId, DateTime startTime)
        {
            var now = DateTime.UtcNow;
            decimal discountPercent = 0;

            if (venueId.HasValue)
            {
                var venueDiscounts = await _discountRepository.GetByVenueIdAsync(venueId.Value);
                var activeVenueDiscount = venueDiscounts
                    .FirstOrDefault(d => d.IsActive && 
                                        d.ValidFrom <= now && 
                                        d.ValidTo >= now &&
                                        d.ValidFrom <= startTime &&
                                        d.ValidTo >= startTime);
                
                if (activeVenueDiscount != null && activeVenueDiscount.PercentOff > discountPercent)
                {
                    discountPercent = activeVenueDiscount.PercentOff;
                }
            }

            if (courtId.HasValue)
            {
                var courtDiscounts = await _discountRepository.GetByCourtIdAsync(courtId.Value);
                var activeCourtDiscount = courtDiscounts
                    .FirstOrDefault(d => d.IsActive && 
                                        d.ValidFrom <= now && 
                                        d.ValidTo >= now &&
                                        d.ValidFrom <= startTime &&
                                        d.ValidTo >= startTime);
                
                if (activeCourtDiscount != null && activeCourtDiscount.PercentOff > discountPercent)
                {
                    discountPercent = activeCourtDiscount.PercentOff;
                }
            }

            // Convert discount percent to factor (e.g., 10% off = 0.9 factor)
            return 1.0m - (discountPercent / 100.0m);
        }
    }
}

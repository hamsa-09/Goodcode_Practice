using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Assignment_Example_HU.DTOs;
using Assignment_Example_HU.Enums;
using Assignment_Example_HU.Models;
using Assignment_Example_HU.Repositories.Interfaces;
using Assignment_Example_HU.Services.Interfaces;

namespace Assignment_Example_HU.Services
{
    public class SlotService : ISlotService
    {
        private readonly ISlotRepository _slotRepository;
        private readonly IPricingService _pricingService;
        private readonly IDistributedLockService _lockService;
        private readonly IDemandTrackingService _demandTrackingService;
        private readonly IMapper _mapper;
        private readonly TimeSpan _priceLockDuration = TimeSpan.FromMinutes(5);
        private readonly TimeSpan _bookingLockDuration = TimeSpan.FromMinutes(10);

        public SlotService(
            ISlotRepository slotRepository,
            IPricingService pricingService,
            IDistributedLockService lockService,
            IDemandTrackingService demandTrackingService,
            IMapper mapper)
        {
            _slotRepository = slotRepository;
            _pricingService = pricingService;
            _lockService = lockService;
            _demandTrackingService = demandTrackingService;
            _mapper = mapper;
        }

        public async Task<IEnumerable<AvailableSlotDto>> GetAvailableSlotsAsync(
            Guid? courtId = null,
            Guid? venueId = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var slots = await _slotRepository.GetAvailableSlotsAsync(courtId, venueId, startDate, endDate);
            var availableSlots = new List<AvailableSlotDto>();

            foreach (var slot in slots)
            {
                // Track viewer for demand calculation
                await _demandTrackingService.IncrementViewerCountAsync(slot.Id);

                // Get base price from court
                var basePrice = slot.Court?.BasePrice ?? slot.Price;

                var priceCalculation = await _pricingService.CalculatePriceAsync(
                    slot.Id,
                    basePrice,
                    slot.StartTime,
                    slot.CourtId,
                    slot.Court?.VenueId);

                var dto = _mapper.Map<AvailableSlotDto>(slot);
                dto.BasePrice = basePrice;
                dto.FinalPrice = priceCalculation.FinalPrice;
                dto.ViewersCount = await _demandTrackingService.GetViewerCountAsync(slot.Id);
                dto.IsPriceLocked = slot.LockedUntil.HasValue && slot.LockedUntil.Value > DateTime.UtcNow;
                dto.PriceLockExpiresAt = slot.LockedUntil;

                availableSlots.Add(dto);
            }

            return availableSlots;
        }

        public async Task<AvailableSlotDto?> GetSlotDetailsAsync(Guid slotId)
        {
            var slot = await _slotRepository.GetByIdWithCourtAsync(slotId);
            if (slot == null) return null;

            // Track viewer
            await _demandTrackingService.IncrementViewerCountAsync(slotId);

            // Get base price from court
            var basePrice = slot.Court?.BasePrice ?? slot.Price;

            var priceCalculation = await _pricingService.CalculatePriceAsync(
                slotId,
                basePrice,
                slot.StartTime,
                slot.CourtId,
                slot.Court?.VenueId);

            var dto = _mapper.Map<AvailableSlotDto>(slot);
            dto.BasePrice = basePrice;
            dto.FinalPrice = priceCalculation.FinalPrice;
            dto.ViewersCount = await _demandTrackingService.GetViewerCountAsync(slotId);
            dto.IsPriceLocked = slot.LockedUntil.HasValue && slot.LockedUntil.Value > DateTime.UtcNow;
            dto.PriceLockExpiresAt = slot.LockedUntil;

            return dto;
        }

        public async Task<BookSlotResponseDto> LockSlotAsync(Guid slotId, Guid userId)
        {
            var lockKey = $"slot_{slotId}_user_{userId}";

            // Acquire distributed lock
            var lockAcquired = await _lockService.AcquireLockAsync(lockKey, _bookingLockDuration);
            if (!lockAcquired)
            {
                throw new InvalidOperationException("Slot is currently being processed by another user.");
            }

            try
            {
                var slot = await _slotRepository.GetByIdAsync(slotId);
                if (slot == null)
                {
                    throw new InvalidOperationException("Slot not found.");
                }

                if (slot.Status != SlotStatus.Available)
                {
                    throw new InvalidOperationException($"Slot is not available. Current status: {slot.Status}");
                }

                // Get base price from court
                var basePrice = slot.Court?.BasePrice ?? slot.Price;

                // Calculate final price and lock it
                var priceCalculation = await _pricingService.CalculatePriceAsync(
                    slotId,
                    basePrice,
                    slot.StartTime,
                    slot.CourtId,
                    slot.Court?.VenueId);

                // Lock the slot with price
                slot.Status = SlotStatus.Locked;
                slot.LockedUntil = DateTime.UtcNow.Add(_priceLockDuration);
                slot.Price = priceCalculation.FinalPrice; // Update with final calculated price
                slot.BookedByUserId = userId;
                slot.BookedAt = DateTime.UtcNow;

                await _slotRepository.UpdateAsync(slot);
                await _slotRepository.SaveChangesAsync();

                return _mapper.Map<BookSlotResponseDto>(slot);
            }
            catch
            {
                // Release lock on error
                await _lockService.ReleaseLockAsync(lockKey);
                throw;
            }
        }

        public async Task<BookSlotResponseDto> ConfirmBookingAsync(Guid slotId, Guid userId)
        {
            var lockKey = $"slot_{slotId}_user_{userId}";

            // Check if user has the lock
            var hasLock = await _lockService.IsLockedAsync(lockKey);
            if (!hasLock)
            {
                throw new InvalidOperationException("You must lock the slot first before confirming.");
            }

            try
            {
                var slot = await _slotRepository.GetByIdAsync(slotId);
                if (slot == null)
                {
                    throw new InvalidOperationException("Slot not found.");
                }

                if (slot.Status != SlotStatus.Locked || slot.BookedByUserId != userId)
                {
                    throw new InvalidOperationException("Slot is not locked by you or lock has expired.");
                }

                if (slot.LockedUntil.HasValue && slot.LockedUntil.Value < DateTime.UtcNow)
                {
                    throw new InvalidOperationException("Price lock has expired. Please lock the slot again.");
                }

                // Confirm booking
                slot.Status = SlotStatus.Booked;
                slot.LockedUntil = null; // Clear lock time after confirmation

                await _slotRepository.UpdateAsync(slot);
                await _slotRepository.SaveChangesAsync();

                // Release distributed lock
                await _lockService.ReleaseLockAsync(lockKey);

                return _mapper.Map<BookSlotResponseDto>(slot);
            }
            catch
            {
                await _lockService.ReleaseLockAsync(lockKey);
                throw;
            }
        }

        public async Task<bool> ReleaseLockAsync(Guid slotId, Guid userId)
        {
            var lockKey = $"slot_{slotId}_user_{userId}";
            var hasLock = await _lockService.IsLockedAsync(lockKey);

            if (!hasLock)
            {
                return false;
            }

            var slot = await _slotRepository.GetByIdAsync(slotId);
            if (slot != null && slot.Status == SlotStatus.Locked && slot.BookedByUserId == userId)
            {
                slot.Status = SlotStatus.Available;
                slot.LockedUntil = null;
                slot.BookedByUserId = null;
                slot.BookedAt = null;
                slot.Price = 0; // Reset price

                await _slotRepository.UpdateAsync(slot);
                await _slotRepository.SaveChangesAsync();
            }

            await _lockService.ReleaseLockAsync(lockKey);
            return true;
        }

        public async Task<bool> CancelBookingAsync(Guid slotId, Guid userId)
        {
            var slot = await _slotRepository.GetByIdAsync(slotId);
            if (slot == null)
            {
                throw new InvalidOperationException("Slot not found.");
            }

            if (slot.BookedByUserId != userId)
            {
                throw new UnauthorizedAccessException("You can only cancel your own bookings.");
            }

            if (slot.Status != SlotStatus.Booked)
            {
                throw new InvalidOperationException("Slot is not booked.");
            }

            slot.Status = SlotStatus.Cancelled;
            slot.BookedByUserId = null;
            slot.BookedAt = null;

            await _slotRepository.UpdateAsync(slot);
            await _slotRepository.SaveChangesAsync();

            return true;
        }

        public async Task ExpireLocksAsync()
        {
            var expiredLocks = await _slotRepository.GetExpiredLocksAsync();

            foreach (var slot in expiredLocks)
            {
                slot.Status = SlotStatus.Available;
                slot.LockedUntil = null;
                slot.BookedByUserId = null;
                slot.BookedAt = null;
                slot.Price = 0;

                await _slotRepository.UpdateAsync(slot);

                // Release distributed lock
                var lockKey = $"slot_{slot.Id}_user_{slot.BookedByUserId}";
                await _lockService.ReleaseLockAsync(lockKey);
            }

            await _slotRepository.SaveChangesAsync();
        }
    }
}

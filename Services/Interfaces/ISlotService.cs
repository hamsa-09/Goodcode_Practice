using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assignment_Example_HU.DTOs;

namespace Assignment_Example_HU.Services.Interfaces
{
    public interface ISlotService
    {
        Task<IEnumerable<AvailableSlotDto>> GetAvailableSlotsAsync(
            Guid? courtId = null,
            Guid? venueId = null,
            DateTime? startDate = null,
            DateTime? endDate = null);

        Task<AvailableSlotDto?> GetSlotDetailsAsync(Guid slotId);
        Task<BookSlotResponseDto> LockSlotAsync(Guid slotId, Guid userId);
        Task<BookSlotResponseDto> ConfirmBookingAsync(Guid slotId, Guid userId);
        Task<bool> ReleaseLockAsync(Guid slotId, Guid userId);
        Task<bool> CancelBookingAsync(Guid slotId, Guid userId);
        Task ExpireLocksAsync();
    }
}

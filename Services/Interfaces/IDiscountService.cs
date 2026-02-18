using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assignment_Example_HU.DTOs;

namespace Assignment_Example_HU.Services.Interfaces
{
    public interface IDiscountService
    {
        Task<DiscountDto> CreateDiscountAsync(CreateDiscountDto dto, Guid userId);
        Task<DiscountDto?> GetDiscountByIdAsync(Guid id);
        Task<IEnumerable<DiscountDto>> GetDiscountsByVenueIdAsync(Guid venueId);
        Task<IEnumerable<DiscountDto>> GetDiscountsByCourtIdAsync(Guid courtId);
        Task<DiscountDto> UpdateDiscountAsync(Guid discountId, UpdateDiscountDto dto, Guid userId);
        Task<bool> DeleteDiscountAsync(Guid discountId, Guid userId);
    }
}

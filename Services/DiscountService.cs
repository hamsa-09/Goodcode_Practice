using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Assignment_Example_HU.DTOs;
using Assignment_Example_HU.Models;
using Assignment_Example_HU.Repositories.Interfaces;
using Assignment_Example_HU.Services.Interfaces;

namespace Assignment_Example_HU.Services
{
    public class DiscountService : IDiscountService
    {
        private readonly IDiscountRepository _discountRepository;
        private readonly IVenueRepository _venueRepository;
        private readonly ICourtRepository _courtRepository;
        private readonly IVenueService _venueService;
        private readonly ICourtService _courtService;
        private readonly IMapper _mapper;

        public DiscountService(
            IDiscountRepository discountRepository,
            IVenueRepository venueRepository,
            ICourtRepository courtRepository,
            IVenueService venueService,
            ICourtService courtService,
            IMapper mapper)
        {
            _discountRepository = discountRepository;
            _venueRepository = venueRepository;
            _courtRepository = courtRepository;
            _venueService = venueService;
            _courtService = courtService;
            _mapper = mapper;
        }

        public async Task<DiscountDto> CreateDiscountAsync(CreateDiscountDto dto, Guid userId)
        {
            if (dto.Scope == Enums.DiscountScope.Venue && dto.VenueId.HasValue)
            {
                if (!await _venueService.IsVenueOwnerAsync(dto.VenueId.Value, userId))
                {
                    throw new UnauthorizedAccessException("You can only create discounts for your own venues.");
                }
            }
            else if (dto.Scope == Enums.DiscountScope.Court && dto.CourtId.HasValue)
            {
                if (!await _courtService.IsCourtOwnerAsync(dto.CourtId.Value, userId))
                {
                    throw new UnauthorizedAccessException("You can only create discounts for your own courts.");
                }
            }

            var discount = _mapper.Map<Discount>(dto);
            discount.Id = Guid.NewGuid();
            discount.IsActive = true;

            await _discountRepository.AddAsync(discount);
            await _discountRepository.SaveChangesAsync();

            return _mapper.Map<DiscountDto>(discount);
        }

        public async Task<DiscountDto?> GetDiscountByIdAsync(Guid id)
        {
            var discount = await _discountRepository.GetByIdAsync(id);
            if (discount == null) return null;

            return _mapper.Map<DiscountDto>(discount);
        }

        public async Task<IEnumerable<DiscountDto>> GetDiscountsByVenueIdAsync(Guid venueId)
        {
            var discounts = await _discountRepository.GetByVenueIdAsync(venueId);
            return _mapper.Map<IEnumerable<DiscountDto>>(discounts);
        }

        public async Task<IEnumerable<DiscountDto>> GetDiscountsByCourtIdAsync(Guid courtId)
        {
            var discounts = await _discountRepository.GetByCourtIdAsync(courtId);
            return _mapper.Map<IEnumerable<DiscountDto>>(discounts);
        }

        public async Task<DiscountDto> UpdateDiscountAsync(Guid discountId, UpdateDiscountDto dto, Guid userId)
        {
            var discount = await _discountRepository.GetByIdAsync(discountId);
            if (discount == null)
            {
                throw new InvalidOperationException("Discount not found.");
            }

            // Check ownership
            if (discount.VenueId.HasValue)
            {
                if (!await _venueService.IsVenueOwnerAsync(discount.VenueId.Value, userId))
                {
                    throw new UnauthorizedAccessException("You can only update discounts for your own venues.");
                }
            }
            else if (discount.CourtId.HasValue)
            {
                if (!await _courtService.IsCourtOwnerAsync(discount.CourtId.Value, userId))
                {
                    throw new UnauthorizedAccessException("You can only update discounts for your own courts.");
                }
            }

            _mapper.Map(dto, discount);

            await _discountRepository.UpdateAsync(discount);
            await _discountRepository.SaveChangesAsync();

            return _mapper.Map<DiscountDto>(discount);
        }

        public async Task<bool> DeleteDiscountAsync(Guid discountId, Guid userId)
        {
            var discount = await _discountRepository.GetByIdAsync(discountId);
            if (discount == null)
            {
                throw new InvalidOperationException("Discount not found.");
            }

            // Check ownership
            if (discount.VenueId.HasValue)
            {
                if (!await _venueService.IsVenueOwnerAsync(discount.VenueId.Value, userId))
                {
                    throw new UnauthorizedAccessException("You can only delete discounts for your own venues.");
                }
            }
            else if (discount.CourtId.HasValue)
            {
                if (!await _courtService.IsCourtOwnerAsync(discount.CourtId.Value, userId))
                {
                    throw new UnauthorizedAccessException("You can only delete discounts for your own courts.");
                }
            }

            await _discountRepository.SaveChangesAsync();
            return true;
        }
    }
}

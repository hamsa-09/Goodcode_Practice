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
    public class CourtService : ICourtService
    {
        private readonly ICourtRepository _courtRepository;
        private readonly IVenueRepository _venueRepository;
        private readonly IVenueService _venueService;
        private readonly IMapper _mapper;

        public CourtService(
            ICourtRepository courtRepository,
            IVenueRepository venueRepository,
            IVenueService venueService,
            IMapper mapper)
        {
            _courtRepository = courtRepository;
            _venueRepository = venueRepository;
            _venueService = venueService;
            _mapper = mapper;
        }

        public async Task<CourtDto> CreateCourtAsync(CreateCourtDto dto, Guid venueOwnerId)
        {
            var venue = await _venueRepository.GetByIdAsync(dto.VenueId);
            if (venue == null)
            {
                throw new InvalidOperationException("Venue not found.");
            }

            if (!await _venueService.IsVenueOwnerAsync(dto.VenueId, venueOwnerId))
            {
                throw new UnauthorizedAccessException("You can only create courts for your own venues.");
            }

            var court = _mapper.Map<Court>(dto);
            court.Id = Guid.NewGuid();
            court.IsActive = true;

            await _courtRepository.AddAsync(court);
            await _courtRepository.SaveChangesAsync();

            var courtWithVenue = await _courtRepository.GetByIdWithVenueAsync(court.Id);
            return _mapper.Map<CourtDto>(courtWithVenue!);
        }

        public async Task<CourtDto?> GetCourtByIdAsync(Guid id)
        {
            var court = await _courtRepository.GetByIdWithVenueAsync(id);
            if (court == null) return null;

            return _mapper.Map<CourtDto>(court);
        }

        public async Task<IEnumerable<CourtDto>> GetCourtsByVenueIdAsync(Guid venueId)
        {
            var courts = await _courtRepository.GetByVenueIdAsync(venueId);
            return _mapper.Map<IEnumerable<CourtDto>>(courts);
        }

        public async Task<IEnumerable<CourtDto>> GetAllCourtsAsync()
        {
            var courts = await _courtRepository.GetAllAsync();
            var courtDtos = new List<CourtDto>();

            foreach (var court in courts)
            {
                var courtWithVenue = await _courtRepository.GetByIdWithVenueAsync(court.Id);
                if (courtWithVenue != null)
                {
                    courtDtos.Add(_mapper.Map<CourtDto>(courtWithVenue));
                }
            }

            return courtDtos;
        }

        public async Task<CourtDto> UpdateCourtAsync(Guid courtId, UpdateCourtDto dto, Guid userId)
        {
            var court = await _courtRepository.GetByIdAsync(courtId);
            if (court == null)
            {
                throw new InvalidOperationException("Court not found.");
            }

            if (!await IsCourtOwnerAsync(courtId, userId))
            {
                throw new UnauthorizedAccessException("You can only update courts in your own venues.");
            }

            _mapper.Map(dto, court);

            await _courtRepository.UpdateAsync(court);
            await _courtRepository.SaveChangesAsync();

            var courtWithVenue = await _courtRepository.GetByIdWithVenueAsync(courtId);
            return _mapper.Map<CourtDto>(courtWithVenue!);
        }

        public async Task<bool> DeleteCourtAsync(Guid courtId, Guid userId)
        {
            var court = await _courtRepository.GetByIdAsync(courtId);
            if (court == null)
            {
                throw new InvalidOperationException("Court not found.");
            }

            if (!await IsCourtOwnerAsync(courtId, userId))
            {
                throw new UnauthorizedAccessException("You can only delete courts in your own venues.");
            }

            await _courtRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsCourtOwnerAsync(Guid courtId, Guid userId)
        {
            var court = await _courtRepository.GetByIdWithVenueAsync(courtId);
            if (court == null) return false;

            return await _venueService.IsVenueOwnerAsync(court.VenueId, userId);
        }
    }
}

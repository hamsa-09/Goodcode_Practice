using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using AutoMapper;
using Assignment_Example_HU.DTOs;
using Assignment_Example_HU.Enums;
using Assignment_Example_HU.Exceptions;
using Assignment_Example_HU.Models;
using Assignment_Example_HU.Repositories.Interfaces;
using Assignment_Example_HU.Services.Interfaces;

namespace Assignment_Example_HU.Services
{
    public class VenueService : IVenueService
    {
        private readonly IVenueRepository _venueRepository;
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;

        public VenueService(
            IVenueRepository venueRepository,
            UserManager<User> userManager,
            IMapper mapper)
        {
            _venueRepository = venueRepository;
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<VenueDto> CreateVenueAsync(CreateVenueDto dto, Guid ownerId)
        {
            var owner = await _userManager.FindByIdAsync(ownerId.ToString());
            if (owner == null)
            {
                throw new NotFoundException("Owner not found.");
            }

            if (owner.Role != Role.VenueOwner && owner.Role != Role.Admin)
            {
                throw new ForbiddenException("Only VenueOwner or Admin can create venues.");
            }

            var venue = _mapper.Map<Venue>(dto);
            venue.Id = Guid.NewGuid();
            venue.OwnerId = ownerId;
            venue.ApprovalStatus = ApprovalStatus.Pending;

            await _venueRepository.AddAsync(venue);
            await _venueRepository.SaveChangesAsync();

            return _mapper.Map<VenueDto>(venue);
        }

        public async Task<VenueDto?> GetVenueByIdAsync(Guid id)
        {
            var venue = await _venueRepository.GetByIdWithOwnerAsync(id);
            if (venue == null) return null;

            return _mapper.Map<VenueDto>(venue);
        }

        public async Task<IEnumerable<VenueDto>> GetVenuesByOwnerAsync(Guid ownerId)
        {
            var venues = await _venueRepository.GetByOwnerIdAsync(ownerId);
            return _mapper.Map<IEnumerable<VenueDto>>(venues);
        }

        public async Task<IEnumerable<VenueDto>> GetAllVenuesAsync()
        {
            var venues = await _venueRepository.GetAllAsync();
            var venueDtos = new List<VenueDto>();

            foreach (var venue in venues)
            {
                var venueWithOwner = await _venueRepository.GetByIdWithOwnerAsync(venue.Id);
                if (venueWithOwner != null)
                {
                    venueDtos.Add(_mapper.Map<VenueDto>(venueWithOwner));
                }
            }

            return venueDtos;
        }

        public async Task<VenueDto> UpdateVenueAsync(Guid venueId, UpdateVenueDto dto, Guid userId)
        {
            var venue = await _venueRepository.GetByIdAsync(venueId);
            if (venue == null)
            {
                throw new NotFoundException("Venue not found.");
            }

            if (!await IsVenueOwnerAsync(venueId, userId))
            {
                throw new ForbiddenException("You can only access your own venues.");
            }

            _mapper.Map(dto, venue);

            await _venueRepository.UpdateAsync(venue);
            await _venueRepository.SaveChangesAsync();

            var venueWithOwner = await _venueRepository.GetByIdWithOwnerAsync(venueId);
            return _mapper.Map<VenueDto>(venueWithOwner!);
        }

        public async Task<bool> UpdateVenueApprovalAsync(Guid venueId, UpdateVenueApprovalDto dto)
        {
            var venue = await _venueRepository.GetByIdAsync(venueId);
            if (venue == null)
            {
                throw new NotFoundException("Venue not found.");
            }

            venue.ApprovalStatus = dto.ApprovalStatus;
            await _venueRepository.UpdateAsync(venue);
            await _venueRepository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteVenueAsync(Guid venueId, Guid userId)
        {
            var venue = await _venueRepository.GetByIdAsync(venueId);
            if (venue == null)
            {
                throw new NotFoundException("Venue not found.");
            }

            if (!await IsVenueOwnerAsync(venueId, userId))
            {
                throw new ForbiddenException("You can only delete your own venues.");
            }

            // Note: In a real app, you might want soft delete or check for dependencies
            // For now, we'll rely on cascade delete from DbContext
            await _venueRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsVenueOwnerAsync(Guid venueId, Guid userId)
        {
            var venue = await _venueRepository.GetByIdAsync(venueId);
            if (venue == null) return false;

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return false;

            return venue.OwnerId == userId || user.Role == Role.Admin;
        }
    }
}

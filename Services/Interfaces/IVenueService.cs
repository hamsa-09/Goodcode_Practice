using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assignment_Example_HU.DTOs;

namespace Assignment_Example_HU.Services.Interfaces
{
    public interface IVenueService
    {
        Task<VenueDto> CreateVenueAsync(CreateVenueDto dto, Guid ownerId);
        Task<VenueDto?> GetVenueByIdAsync(Guid id);
        Task<IEnumerable<VenueDto>> GetVenuesByOwnerAsync(Guid ownerId);
        Task<IEnumerable<VenueDto>> GetAllVenuesAsync();
        Task<VenueDto> UpdateVenueAsync(Guid venueId, UpdateVenueDto dto, Guid userId);
        Task<bool> UpdateVenueApprovalAsync(Guid venueId, UpdateVenueApprovalDto dto);
        Task<bool> DeleteVenueAsync(Guid venueId, Guid userId);
        Task<bool> IsVenueOwnerAsync(Guid venueId, Guid userId);
    }
}

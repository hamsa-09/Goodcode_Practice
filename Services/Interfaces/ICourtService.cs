using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assignment_Example_HU.DTOs;

namespace Assignment_Example_HU.Services.Interfaces
{
    public interface ICourtService
    {
        Task<CourtDto> CreateCourtAsync(CreateCourtDto dto, Guid venueOwnerId);
        Task<CourtDto?> GetCourtByIdAsync(Guid id);
        Task<IEnumerable<CourtDto>> GetCourtsByVenueIdAsync(Guid venueId);
        Task<IEnumerable<CourtDto>> GetAllCourtsAsync();
        Task<CourtDto> UpdateCourtAsync(Guid courtId, UpdateCourtDto dto, Guid userId);
        Task<bool> DeleteCourtAsync(Guid courtId, Guid userId);
        Task<bool> IsCourtOwnerAsync(Guid courtId, Guid userId);
    }
}

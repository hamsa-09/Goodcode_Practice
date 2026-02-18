using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assignment_Example_HU.DTOs;

namespace Assignment_Example_HU.Services.Interfaces
{
    public interface IWaitlistService
    {
        Task<WaitlistDto> JoinWaitlistAsync(Guid gameId, Guid userId);
        Task<bool> LeaveWaitlistAsync(Guid gameId, Guid userId);
        Task<IEnumerable<WaitlistDto>> GetWaitlistByGameAsync(Guid gameId);
        Task<bool> InviteFromWaitlistAsync(Guid gameId, Guid userId, Guid invitedUserId);
        Task RemoveWaitlistForGameAsync(Guid gameId);
        Task<int> GetWaitlistCountAsync(Guid gameId);
    }
}

using System;
using System.Threading.Tasks;

namespace Assignment_Example_HU.Services.Interfaces
{
    public interface IDemandTrackingService
    {
        Task IncrementViewerCountAsync(Guid slotId);
        Task<int> GetViewerCountAsync(Guid slotId);
        Task ResetViewerCountAsync(Guid slotId);
    }
}

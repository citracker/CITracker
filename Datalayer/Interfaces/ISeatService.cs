using Shared.DTO;

namespace Datalayer.Interfaces
{
    public interface ISeatService
    {
        Task<ResponseHandler> CanInviteAsync(int organizationId);
        Task<ResponseHandler> ReserveSeatAsync(int organizationId, long newUserId);
        Task<ResponseHandler> ReleaseSeatAsync(int organizationId, long userId);
        Task<ResponseHandler> SetSeatsPurchasedAsync(int organizationId, int seatsPurchased);
        Task<ResponseHandler> ScheduleSeatChangeAtRenewalAsync(int organizationId, int? seats, int? newPlanId);
    }
}

using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.Schedule;
using FRPAMSystem.DataTier.Paginate;

namespace FRPAMSystem.BusinessTier.Services.Interface
{
    public interface IScheduleService
    {
        Task<IPaginate<ScheduleResponse>> ViewAllSchedulesAsync(
            ScheduleFilter filter,
            PagingModel pagingModel);

        Task<IPaginate<ScheduleResponse>> ViewMineAsync(
            int userId,
            ScheduleFilter filter,
            PagingModel pagingModel);

        Task<ScheduleResponse?> GetScheduleByIdAsync(int id);

        Task<ScheduleResponse?> GetScheduleByIdForUserAsync(int id, int userId);

        Task<ScheduleResponse> CreateScheduleAsync(ScheduleRequest request);

        Task<ScheduleResponse?> UpdateScheduleAsync(int id, ScheduleRequest request);

        Task<bool> DeleteScheduleAsync(int id);

        Task<ScheduleResponse?> CompleteScheduleAsync(int id, int userId, CompleteScheduleRequest? request = null);

        Task<ScheduleResponse?> CancelScheduleAsync(int id, int userId, CancelScheduleRequest? request = null);

        Task AutoSyncInProgressSchedulesAsync();
    }
}

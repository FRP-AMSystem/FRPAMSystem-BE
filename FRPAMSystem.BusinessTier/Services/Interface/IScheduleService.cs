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

        Task<ScheduleResponse?> GetScheduleByIdAsync(int id);

        Task<ScheduleResponse> CreateScheduleAsync(ScheduleRequest request);

        Task<ScheduleResponse?> UpdateScheduleAsync(int id, ScheduleRequest request);

        Task<bool> DeleteScheduleAsync(int id);
    }
}

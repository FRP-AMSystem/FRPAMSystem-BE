using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.AllocationPlan;
using FRPAMSystem.DataTier.Paginate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.Services.Interface
{
    public interface IAllocationPlanService
    {
        Task<IPaginate<AllocationPlanResponse>> ViewAllAllocationPlansAsync(
            AllocationPlanFilter filter,
            PagingModel pagingModel);

        Task<AllocationPlanResponse?> GetAllocationPlanByIdAsync(int id);

        Task<AllocationPlanResponse> CreateAllocationPlanAsync(
            AllocationPlanRequest request,
            int? currentUserId);

        Task<AllocationPlanResponse?> UpdateAllocationPlanAsync(
            int id,
            AllocationPlanRequest request);

        Task<bool> DeleteAllocationPlanAsync(int id);

        Task<AllocationPlanResponse?> ApproveAllocationPlanAsync(
            int id,
            int? currentUserId);

        Task<AllocationPlanResponse?> RejectAllocationPlanAsync(
            int id,
            int? currentUserId);

        Task<AllocationPlanResponse?> CancelAllocationPlanAsync(
            int id);
    }
}

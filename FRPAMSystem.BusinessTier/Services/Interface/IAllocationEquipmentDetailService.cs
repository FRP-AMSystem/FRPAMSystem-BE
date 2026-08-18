using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.AllocationEquipmentDetail;
using FRPAMSystem.DataTier.Paginate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.Services.Interface
{
    public interface IAllocationEquipmentDetailService
    {
        Task<IPaginate<AllocationEquipmentDetailResponse>> ViewAllAllocationEquipmentDetailsAsync(
            AllocationEquipmentDetailFilter filter,
            PagingModel pagingModel);

        Task<IPaginate<AllocationEquipmentDetailResponse>> ViewMineAsync(
            int userId,
            AllocationEquipmentDetailFilter filter,
            PagingModel pagingModel);

        Task<AllocationEquipmentDetailResponse?> GetAllocationEquipmentDetailByIdAsync(int id);

        Task<AllocationEquipmentDetailResponse?> GetAllocationEquipmentDetailByIdForUserAsync(
            int id,
            int userId);

        Task<AllocationEquipmentDetailResponse?> HandoverMineAsync(int id, int userId);

        Task<AllocationEquipmentDetailResponse?> ReturnMineAsync(int id, int userId);

        Task<AllocationEquipmentDetailResponse> CreateAllocationEquipmentDetailAsync(
            AllocationEquipmentDetailRequest request);

        Task<AllocationEquipmentDetailResponse?> UpdateAllocationEquipmentDetailAsync(
            int id,
            AllocationEquipmentDetailRequest request);

        Task<bool> DeleteAllocationEquipmentDetailAsync(int id);
    }
}

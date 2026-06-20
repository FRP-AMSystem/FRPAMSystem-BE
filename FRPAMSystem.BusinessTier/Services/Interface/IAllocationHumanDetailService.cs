using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.AllocationHumanDetail;
using FRPAMSystem.DataTier.Paginate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.Services.Interface
{
    public interface IAllocationHumanDetailService
    {
        Task<IPaginate<AllocationHumanDetailResponse>> ViewAllAllocationHumanDetailsAsync(
            AllocationHumanDetailFilter filter,
            PagingModel pagingModel);

        Task<AllocationHumanDetailResponse?> GetAllocationHumanDetailByIdAsync(int id);

        Task<AllocationHumanDetailResponse> CreateAllocationHumanDetailAsync(
            AllocationHumanDetailRequest request);

        Task<AllocationHumanDetailResponse?> UpdateAllocationHumanDetailAsync(
            int id,
            AllocationHumanDetailRequest request);

        Task<bool> DeleteAllocationHumanDetailAsync(int id);
    }
}

using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.AllocationLandDetail;
using FRPAMSystem.DataTier.Paginate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.Services.Interface
{
    public interface IAllocationLandDetailService
    {
        Task<IPaginate<AllocationLandDetailResponse>> ViewAllAllocationLandDetailsAsync(
            AllocationLandDetailFilter filter,
            PagingModel pagingModel);

        Task<AllocationLandDetailResponse?> GetAllocationLandDetailByIdAsync(int id);

        Task<AllocationLandDetailResponse> CreateAllocationLandDetailAsync(
            AllocationLandDetailRequest request);

        Task<AllocationLandDetailResponse?> UpdateAllocationLandDetailAsync(
            int id,
            AllocationLandDetailRequest request);

        Task<bool> DeleteAllocationLandDetailAsync(int id);
    }
}

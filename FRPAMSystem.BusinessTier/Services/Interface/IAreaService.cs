using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.Area;
using FRPAMSystem.DataTier.Paginate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.Services.Interface
{
    public interface IAreaService
    {
        Task<IPaginate<AreaResponse>> ViewAllAreasAsync(
             AreaFilter filter,
             PagingModel pagingModel);

        Task<AreaResponse?> GetAreaByIdAsync(int id);

        Task<AreaResponse> CreateAreaAsync(AreaRequest request);

        Task<AreaResponse?> UpdateAreaAsync(int id, AreaRequest request);

        Task<bool> DeleteAreaAsync(int id);
    }
}

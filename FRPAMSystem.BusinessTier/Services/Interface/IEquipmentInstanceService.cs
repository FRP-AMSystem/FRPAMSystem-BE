using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.EquipmentInstances;
using FRPAMSystem.DataTier.Paginate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.Services.Interface
{
    public interface IEquipmentInstanceService
    {
        Task<IPaginate<EquipmentInstanceResponse>> ViewAllEquipmentInstancesAsync(
            EquipmentInstanceFilter filter,
            PagingModel pagingModel);

        Task<EquipmentInstanceResponse?> GetEquipmentInstanceByIdAsync(int id);

        Task<EquipmentInstanceResponse> CreateEquipmentInstanceAsync(
            EquipmentInstanceRequest request);

        Task<EquipmentInstanceResponse?> UpdateEquipmentInstanceAsync(
            int id,
            EquipmentInstanceRequest request);

        Task<bool> DeleteEquipmentInstanceAsync(int id);
    }
}

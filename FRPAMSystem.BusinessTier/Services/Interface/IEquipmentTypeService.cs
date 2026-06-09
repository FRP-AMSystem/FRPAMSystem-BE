using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.EquipmentType;
using FRPAMSystem.DataTier.Paginate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.Services.Interface
{
    public interface IEquipmentTypeService
    {
        Task<IPaginate<EquipmentTypeResponse>> ViewAllEquipmentTypesAsync(
            EquipmentTypeFilter filter,
            PagingModel pagingModel);

        Task<EquipmentTypeResponse?> GetEquipmentTypeByIdAsync(int id);

        Task<EquipmentTypeResponse> CreateEquipmentTypeAsync(EquipmentTypeRequest request);

        Task<EquipmentTypeResponse?> UpdateEquipmentTypeAsync(int id, EquipmentTypeRequest request);

        Task<bool> DeleteEquipmentTypeAsync(int id);
    }
}

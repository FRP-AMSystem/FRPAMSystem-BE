using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.EquipmentCategory;
using FRPAMSystem.DataTier.Paginate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.Services.Interface
{
    public interface IEquipmentCategoryService
    {
        Task<IPaginate<EquipmentCategoryResponse>> ViewAllEquipmentCategoriesAsync(
            EquipmentCategoryFilter filter,
            PagingModel pagingModel);

        Task<EquipmentCategoryResponse?> GetEquipmentCategoryByIdAsync(int id);

        Task<EquipmentCategoryResponse> CreateEquipmentCategoryAsync(EquipmentCategoryRequest request);

        Task<EquipmentCategoryResponse?> UpdateEquipmentCategoryAsync(int id, EquipmentCategoryRequest request);

        Task<bool> DeleteEquipmentCategoryAsync(int id);
    }
}

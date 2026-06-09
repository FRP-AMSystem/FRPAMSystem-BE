using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.EquipmentCategory;
using FRPAMSystem.BusinessTier.Services.Interface;
using FRPAMSystem.DataTier.Models;
using FRPAMSystem.DataTier.Paginate;
using FRPAMSystem.DataTier.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.Services.Implements
{
    public class EquipmentCategoryService : IEquipmentCategoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public EquipmentCategoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IPaginate<EquipmentCategoryResponse>> ViewAllEquipmentCategoriesAsync(
            EquipmentCategoryFilter filter,
            PagingModel pagingModel)
        {
            PagingModelHelper.NormalizePaging(pagingModel);

            var query = _unitOfWork
                .GetRepository<EquipmentCategory>()
                .GetQueryable()
                .ApplyFilter(filter)
                .AsNoTracking()
                .OrderBy(c => c.CategoryName);

            return await query
                .Select(c => new EquipmentCategoryResponse
                {
                    EquipmentCategoryId = c.EquipmentCategoryId,
                    CategoryName = c.CategoryName,
                    Description = c.Description,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                })
                .ToPaginateAsync(pagingModel.Page, pagingModel.Size, 1);
        }

        public async Task<EquipmentCategoryResponse?> GetEquipmentCategoryByIdAsync(int id)
        {
            var category = await _unitOfWork
                .GetRepository<EquipmentCategory>()
                .FirstOrDefaultAsync(predicate: c => c.EquipmentCategoryId == id);

            if (category == null)
            {
                return null;
            }

            return new EquipmentCategoryResponse
            {
                EquipmentCategoryId = category.EquipmentCategoryId,
                CategoryName = category.CategoryName,
                Description = category.Description,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };
        }

        public async Task<EquipmentCategoryResponse> CreateEquipmentCategoryAsync(
            EquipmentCategoryRequest request)
        {
            var exists = await _unitOfWork
                .GetRepository<EquipmentCategory>()
                .AnyAsync(c => c.CategoryName == request.CategoryName);

            if (exists)
            {
                throw new Exception("Equipment category name already exists.");
            }

            var category = new EquipmentCategory
            {
                CategoryName = request.CategoryName,
                Description = request.Description,
                CreatedAt = DateTime.Now
            };

            await _unitOfWork.GetRepository<EquipmentCategory>().InsertAsync(category);
            await _unitOfWork.CommitAsync();

            return new EquipmentCategoryResponse
            {
                EquipmentCategoryId = category.EquipmentCategoryId,
                CategoryName = category.CategoryName,
                Description = category.Description,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };
        }

        public async Task<EquipmentCategoryResponse?> UpdateEquipmentCategoryAsync(
            int id,
            EquipmentCategoryRequest request)
        {
            var category = await _unitOfWork
                .GetRepository<EquipmentCategory>()
                .FirstOrDefaultAsync(
                    predicate: c => c.EquipmentCategoryId == id,
                    asNoTracking: false
                );

            if (category == null)
            {
                return null;
            }

            var duplicateName = await _unitOfWork
                .GetRepository<EquipmentCategory>()
                .AnyAsync(c =>
                    c.CategoryName == request.CategoryName &&
                    c.EquipmentCategoryId != id);

            if (duplicateName)
            {
                throw new Exception("Equipment category name already exists.");
            }

            category.CategoryName = request.CategoryName;
            category.Description = request.Description;
            category.UpdatedAt = DateTime.Now;

            _unitOfWork.GetRepository<EquipmentCategory>().Update(category);
            await _unitOfWork.CommitAsync();

            return new EquipmentCategoryResponse
            {
                EquipmentCategoryId = category.EquipmentCategoryId,
                CategoryName = category.CategoryName,
                Description = category.Description,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };
        }

        public async Task<bool> DeleteEquipmentCategoryAsync(int id)
        {
            var category = await _unitOfWork
                .GetRepository<EquipmentCategory>()
                .FirstOrDefaultAsync(
                    predicate: c => c.EquipmentCategoryId == id,
                    asNoTracking: false
                );

            if (category == null)
            {
                return false;
            }

            _unitOfWork.GetRepository<EquipmentCategory>().Delete(category);
            await _unitOfWork.CommitAsync();

            return true;
        } 
    }
}

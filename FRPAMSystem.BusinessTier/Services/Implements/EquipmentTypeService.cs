using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.EquipmentType;
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
using FRPAMSystem.BusinessTier.Enums;

namespace FRPAMSystem.BusinessTier.Services.Implements
{
    public class EquipmentTypeService : IEquipmentTypeService
    {

        private readonly IUnitOfWork _unitOfWork;

        public EquipmentTypeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IPaginate<EquipmentTypeResponse>> ViewAllEquipmentTypesAsync(
            EquipmentTypeFilter filter,
            PagingModel pagingModel)
        {
            PagingModelHelper.NormalizePaging(pagingModel);

            var query = _unitOfWork
                .GetRepository<EquipmentType>()
                .GetQueryable()
                .Include(e => e.EquipmentCategory)
                .ApplyFilter(filter)
                .AsNoTracking()
                .OrderBy(e => e.Name);

            return await query
                .Select(e => new EquipmentTypeResponse
                {
                    EquipmentTypeId = e.EquipmentTypeId,
                    EquipmentCategoryId = e.EquipmentCategoryId,
                    EquipmentCategoryName = e.EquipmentCategory.CategoryName,
                    Name = e.Name,
                    TrackingType = e.TrackingType,
                    BaseMaintenanceIntervalHours = e.BaseMaintenanceIntervalHours,
                    TotalQuantity = e.TotalQuantity,
                    DamagedQuantity = e.DamagedQuantity,
                    AvailableQuantity = e.AvailableQuantity,
                    ReservedQuantity = e.ReservedQuantity,
                    InUseQuantity = e.InUseQuantity,
                    MissingQuantity = e.MissingQuantity,
                    Description = e.Description,
                    CreatedAt = e.CreatedAt,
                    UpdatedAt = e.UpdatedAt
                })
                .ToPaginateAsync(pagingModel.Page, pagingModel.Size, 1);
        }

        public async Task<EquipmentTypeResponse?> GetEquipmentTypeByIdAsync(int id)
        {
            var equipmentType = await _unitOfWork
                .GetRepository<EquipmentType>()
                .FirstOrDefaultAsync(
                    predicate: e => e.EquipmentTypeId == id,
                    include: query => query.Include(e => e.EquipmentCategory)
                );

            if (equipmentType == null)
            {
                return null;
            }

            return MapToResponse(equipmentType);
        }

        public async Task<EquipmentTypeResponse> CreateEquipmentTypeAsync(
            EquipmentTypeRequest request)
        {

            ValidateQuantity(request);

            var categoryExists = await _unitOfWork
                .GetRepository<EquipmentCategory>()
                .AnyAsync(c => c.EquipmentCategoryId == request.EquipmentCategoryId);

            if (!categoryExists)
            {
                throw new Exception("Equipment category does not exist.");
            }

            var duplicateName = await _unitOfWork
                .GetRepository<EquipmentType>()
                .AnyAsync(e => e.Name == request.Name);

            if (duplicateName)
            {
                throw new Exception("Equipment type name already exists.");
            }

            var trackingType = request.TrackingType.ToString();

            var equipmentType = new EquipmentType
            {
                EquipmentCategoryId = request.EquipmentCategoryId,
                Name = request.Name,
                TrackingType = trackingType,
                BaseMaintenanceIntervalHours = request.BaseMaintenanceIntervalHours,
                TotalQuantity = request.TotalQuantity,
                DamagedQuantity = 0,
                ReservedQuantity = 0,
                InUseQuantity = 0,
                MissingQuantity = 0,
                Description = request.Description,
                CreatedAt = DateTime.Now
            };

            if (request.TrackingType == EquipmentTrackingType.QuantityBased)
            {
                equipmentType.AvailableQuantity = request.TotalQuantity;
            }
            else
            {
                equipmentType.AvailableQuantity = 0;
                equipmentType.TotalQuantity = 0;
            }

            await _unitOfWork.GetRepository<EquipmentType>().InsertAsync(equipmentType);
            await _unitOfWork.CommitAsync();

            var created = await _unitOfWork
                .GetRepository<EquipmentType>()
                .FirstOrDefaultAsync(
                    predicate: e => e.EquipmentTypeId == equipmentType.EquipmentTypeId,
                    include: query => query.Include(e => e.EquipmentCategory)
                );

            return MapToResponse(created!);
        }

        public async Task<EquipmentTypeResponse?> UpdateEquipmentTypeAsync(
            int id,
            EquipmentTypeRequest request)
        {
          
            ValidateQuantity(request);

            var equipmentType = await _unitOfWork
                .GetRepository<EquipmentType>()
                .FirstOrDefaultAsync(
                    predicate: e => e.EquipmentTypeId == id,
                    asNoTracking: false
                );

            if (equipmentType == null)
            {
                return null;
            }

            var categoryExists = await _unitOfWork
                .GetRepository<EquipmentCategory>()
                .AnyAsync(c => c.EquipmentCategoryId == request.EquipmentCategoryId);

            if (!categoryExists)
            {
                throw new Exception("Equipment category does not exist.");
            }

            var duplicateName = await _unitOfWork
                .GetRepository<EquipmentType>()
                .AnyAsync(e => e.Name == request.Name && e.EquipmentTypeId != id);

            if (duplicateName)
            {
                throw new Exception("Equipment type name already exists.");
            }

            var currentAllocatedQuantity =
                equipmentType.ReservedQuantity +
                equipmentType.InUseQuantity +
                equipmentType.DamagedQuantity +
                equipmentType.MissingQuantity;

            if (request.TrackingType == EquipmentTrackingType.QuantityBased &&
                request.TotalQuantity < currentAllocatedQuantity)
            {
                throw new Exception(
                    "Total quantity cannot be smaller than reserved, in-use, damaged and missing quantity.");
            }

            equipmentType.EquipmentCategoryId = request.EquipmentCategoryId;
            equipmentType.Name = request.Name;
            equipmentType.TrackingType = request.TrackingType.ToString();
            equipmentType.BaseMaintenanceIntervalHours = request.BaseMaintenanceIntervalHours;
            equipmentType.Description = request.Description;
            equipmentType.UpdatedAt = DateTime.Now;

            if (request.TrackingType == EquipmentTrackingType.QuantityBased)
            {
                equipmentType.TotalQuantity = request.TotalQuantity;
                equipmentType.AvailableQuantity = request.TotalQuantity - currentAllocatedQuantity;
            }
            else
            {
                equipmentType.TotalQuantity = 0;
                equipmentType.AvailableQuantity = 0;
                equipmentType.ReservedQuantity = 0;
                equipmentType.InUseQuantity = 0;
                equipmentType.DamagedQuantity = 0;
                equipmentType.MissingQuantity = 0;
            }

            _unitOfWork.GetRepository<EquipmentType>().Update(equipmentType);
            await _unitOfWork.CommitAsync();

            var updated = await _unitOfWork
                .GetRepository<EquipmentType>()
                .FirstOrDefaultAsync(
                    predicate: e => e.EquipmentTypeId == id,
                    include: query => query.Include(e => e.EquipmentCategory)
                );

            return MapToResponse(updated!);
        }

        public async Task<bool> DeleteEquipmentTypeAsync(int id)
        {
            var equipmentType = await _unitOfWork
                .GetRepository<EquipmentType>()
                .FirstOrDefaultAsync(
                    predicate: e => e.EquipmentTypeId == id,
                    asNoTracking: false
                );

            if (equipmentType == null)
            {
                return false;
            }

            var hasInstance = await _unitOfWork
                .GetRepository<EquipmentInstance>()
                .AnyAsync(i => i.EquipmentTypeId == id);

            if (hasInstance)
            {
                throw new Exception("Cannot delete equipment type because it has equipment instances.");
            }

            _unitOfWork.GetRepository<EquipmentType>().Delete(equipmentType);
            await _unitOfWork.CommitAsync();

            return true;
        }

        private static EquipmentTypeResponse MapToResponse(EquipmentType equipmentType)
        {
            return new EquipmentTypeResponse
            {
                EquipmentTypeId = equipmentType.EquipmentTypeId,
                EquipmentCategoryId = equipmentType.EquipmentCategoryId,
                EquipmentCategoryName = equipmentType.EquipmentCategory?.CategoryName,
                Name = equipmentType.Name,
                TrackingType = equipmentType.TrackingType,
                BaseMaintenanceIntervalHours = equipmentType.BaseMaintenanceIntervalHours,
                TotalQuantity = equipmentType.TotalQuantity,
                DamagedQuantity = equipmentType.DamagedQuantity,
                AvailableQuantity = equipmentType.AvailableQuantity,
                ReservedQuantity = equipmentType.ReservedQuantity,
                InUseQuantity = equipmentType.InUseQuantity,
                MissingQuantity = equipmentType.MissingQuantity,
                Description = equipmentType.Description,
                CreatedAt = equipmentType.CreatedAt,
                UpdatedAt = equipmentType.UpdatedAt
            };
        }

        private static void ValidateQuantity(EquipmentTypeRequest request)
        {
            if (request.TotalQuantity < 0)
            {
                throw new Exception("Total quantity cannot be negative.");
            }

            if (request.TrackingType == EquipmentTrackingType.QuantityBased &&
                request.TotalQuantity <= 0)
            {
                throw new Exception(
                    "Quantity-based equipment must have total quantity greater than 0.");
            }

            if (request.TrackingType == EquipmentTrackingType.Individual &&
                request.BaseMaintenanceIntervalHours.HasValue &&
                request.BaseMaintenanceIntervalHours.Value <= 0)
            {
                throw new Exception(
                    "Base maintenance interval hours must be greater than 0.");
            }
        }
    }
}

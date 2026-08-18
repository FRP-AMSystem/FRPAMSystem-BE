using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Enums;
using FRPAMSystem.BusinessTier.Payload.EquipmentInstances;
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
    public class EquipmentInstanceService : IEquipmentInstanceService
    {
        private readonly IUnitOfWork _unitOfWork;

        public EquipmentInstanceService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IPaginate<EquipmentInstanceResponse>> ViewAllEquipmentInstancesAsync(
            EquipmentInstanceFilter filter,
            PagingModel pagingModel)
        {
            PagingModelHelper.NormalizePaging(pagingModel);

            var query = _unitOfWork
                .GetRepository<EquipmentInstance>()
                .GetQueryable()
                .Include(e => e.EquipmentType)
                    .ThenInclude(t => t.EquipmentCategory)
                .ApplyFilter(filter)
                .AsNoTracking()
                .OrderBy(e => e.AssetCode);

            return await query
                .Select(e => new EquipmentInstanceResponse
                {
                    EquipmentInstanceId = e.EquipmentInstanceId,
                    EquipmentTypeId = e.EquipmentTypeId,
                    EquipmentTypeName = e.EquipmentType.Name,
                    EquipmentCategoryId = e.EquipmentType.EquipmentCategoryId,
                    EquipmentCategoryName = e.EquipmentType.EquipmentCategory.CategoryName,
                    AssetCode = e.AssetCode,
                    SerialNumber = e.SerialNumber,
                    TotalUsageHours = e.TotalUsageHour,
                    LastMaintenanceDate = e.LastMaintenanceDate,
                    UsageHoursSinceMaintenance = e.UsageHoursSinceLastMaintenance,
                    ConditionLevel = e.ConditionLevel,
                    Status = e.Status,
                    EffectiveMaintenanceIntervalHours = e.EffectiveIntervalHour,
                    MaintenanceCount = e.MaintenanceCount,
                    Note = e.Note,
                    CreatedAt = e.CreatedAt,
                    UpdatedAt = e.UpdatedAt
                })
                .ToPaginateAsync(pagingModel.Page, pagingModel.Size, 1);
        }

        public async Task<EquipmentInstanceResponse?> GetEquipmentInstanceByIdAsync(int id)
        {
            var equipmentInstance = await _unitOfWork
                .GetRepository<EquipmentInstance>()
                .FirstOrDefaultAsync(
                    predicate: e => e.EquipmentInstanceId == id,
                    include: query => query
                        .Include(e => e.EquipmentType)
                        .ThenInclude(t => t.EquipmentCategory)
                );

            if (equipmentInstance == null)
            {
                return null;
            }

            return MapToResponse(equipmentInstance);
        }

        public async Task<EquipmentInstanceResponse> CreateEquipmentInstanceAsync(
            EquipmentInstanceRequest request)
        {

            ValidateUsageValues(request);
            var assetCode = request.AssetCode.Trim();
            var serialNumber = request.SerialNumber?.Trim();
            var equipmentType = await _unitOfWork
                .GetRepository<EquipmentType>()
                .FirstOrDefaultAsync(
                    predicate: e => e.EquipmentTypeId == request.EquipmentTypeId,
                    include: query => query.Include(e => e.EquipmentCategory),
                    asNoTracking: false
                );

            if (equipmentType == null)
            {
                throw new Exception("Equipment type does not exist.");
            }

            var trackingType = EnumHelper.ParseEnum<EquipmentTrackingType>(equipmentType.TrackingType);

            if (trackingType != EquipmentTrackingType.Individual)
            {
                throw new Exception("Only individual equipment type can have equipment instances.");
            }

            var duplicateAssetCode = await _unitOfWork
                .GetRepository<EquipmentInstance>()
                .AnyAsync(e => e.AssetCode == assetCode);

            if (duplicateAssetCode)
            {
                throw new Exception("Asset code already exists.");
            }

            if (!string.IsNullOrWhiteSpace(request.SerialNumber))
            {
                var duplicateSerialNumber = await _unitOfWork
                    .GetRepository<EquipmentInstance>()
                    .AnyAsync(e => e.SerialNumber == request.SerialNumber);

                if (duplicateSerialNumber)
                {
                    throw new Exception("Serial number already exists.");
                }
            }

            var equipmentInstance = new EquipmentInstance
            {
                EquipmentTypeId = request.EquipmentTypeId,
                AssetCode = assetCode,
                SerialNumber = serialNumber,
                TotalUsageHour = request.TotalUsageHours,
                LastMaintenanceDate = request.LastMaintenanceDate,
                UsageHoursSinceLastMaintenance = request.UsageHoursSinceMaintenance,
                ConditionLevel = request.ConditionLevel.ToString(),
                Status = request.Status.ToString(),
                EffectiveIntervalHour = CalculateEffectiveIntervalHour(
                    equipmentType.BaseMaintenanceIntervalHours ?? 0,
                    request.ConditionLevel,
                    request.MaintenanceCount),
                MaintenanceCount = request.MaintenanceCount,
                Note = request.Note,
                CreatedAt = DateTime.Now
            };

            await _unitOfWork.GetRepository<EquipmentInstance>()
                .InsertAsync(equipmentInstance);

            IncreaseEquipmentTypeQuantityByStatus(equipmentType, request.Status);

            _unitOfWork.GetRepository<EquipmentType>().Update(equipmentType);

            await _unitOfWork.CommitAsync();

            var created = await _unitOfWork
                .GetRepository<EquipmentInstance>()
                .FirstOrDefaultAsync(
                    predicate: e => e.EquipmentInstanceId == equipmentInstance.EquipmentInstanceId,
                    include: query => query
                        .Include(e => e.EquipmentType)
                        .ThenInclude(t => t.EquipmentCategory)
                );

            return MapToResponse(created!);
        }

        public async Task<EquipmentInstanceResponse?> UpdateEquipmentInstanceAsync(
            int id,
            EquipmentInstanceRequest request)
        {
            ValidateUsageValues(request);
            var assetCode = request.AssetCode.Trim();
            var serialNumber = request.SerialNumber?.Trim();
            var equipmentInstance = await _unitOfWork
                .GetRepository<EquipmentInstance>()
                .FirstOrDefaultAsync(
                    predicate: e => e.EquipmentInstanceId == id,
                    include: query => query.Include(e => e.EquipmentType),
                    asNoTracking: false
                );

            if (equipmentInstance == null)
            {
                return null;
            }

            var oldEquipmentTypeId = equipmentInstance.EquipmentTypeId;
            var oldStatus = EnumHelper.ParseEnum<EquipmentInstanceStatus>(equipmentInstance.Status);

            var newEquipmentType = await _unitOfWork
                .GetRepository<EquipmentType>()
                .FirstOrDefaultAsync(
                    predicate: e => e.EquipmentTypeId == request.EquipmentTypeId,
                    asNoTracking: false
                );

            if (newEquipmentType == null)
            {
                throw new Exception("Equipment type does not exist.");
            }

            if (newEquipmentType.TrackingType != EquipmentTrackingType.Individual.ToString())
            {
                throw new Exception("Only individual equipment type can have equipment instances.");
            }

            var duplicateAssetCode = await _unitOfWork
                .GetRepository<EquipmentInstance>()
                .AnyAsync(e => e.AssetCode == assetCode);

            if (duplicateAssetCode)
            {
                throw new Exception("Asset code already exists.");
            }

            if (!string.IsNullOrWhiteSpace(request.SerialNumber))
            {
                var duplicateSerialNumber = await _unitOfWork
                    .GetRepository<EquipmentInstance>()
                    .AnyAsync(e =>
                        e.SerialNumber == request.SerialNumber &&
                        e.EquipmentInstanceId != id);

                if (duplicateSerialNumber)
                {
                    throw new Exception("Serial number already exists.");
                }
            }

            EquipmentType? oldEquipmentType = null;

            if (oldEquipmentTypeId != request.EquipmentTypeId)
            {
                oldEquipmentType = await _unitOfWork
                    .GetRepository<EquipmentType>()
                    .FirstOrDefaultAsync(
                        predicate: e => e.EquipmentTypeId == oldEquipmentTypeId,
                        asNoTracking: false
                    );

                if (oldEquipmentType != null)
                {
                    DecreaseEquipmentTypeQuantityByStatus(oldEquipmentType, oldStatus);
                    _unitOfWork.GetRepository<EquipmentType>().Update(oldEquipmentType);
                }

                IncreaseEquipmentTypeQuantityByStatus(newEquipmentType, request.Status);
            }
            else
            {
                if (oldStatus != request.Status)
                {
                    DecreaseEquipmentTypeQuantityByStatus(newEquipmentType, oldStatus);
                    IncreaseEquipmentTypeQuantityByStatus(newEquipmentType, request.Status);
                }
            }

            if (oldStatus == EquipmentInstanceStatus.Maintenance && request.Status == EquipmentInstanceStatus.Available)
            {
                request.UsageHoursSinceMaintenance = 0;
                request.MaintenanceCount += 1;
            }

            equipmentInstance.EquipmentTypeId = request.EquipmentTypeId;
            equipmentInstance.AssetCode = request.AssetCode;
            equipmentInstance.SerialNumber = request.SerialNumber;
            equipmentInstance.TotalUsageHour = request.TotalUsageHours;
            equipmentInstance.LastMaintenanceDate = request.LastMaintenanceDate;
            equipmentInstance.UsageHoursSinceLastMaintenance = request.UsageHoursSinceMaintenance;
            equipmentInstance.ConditionLevel = request.ConditionLevel.ToString();
            equipmentInstance.Status = request.Status.ToString();
            equipmentInstance.EffectiveIntervalHour = CalculateEffectiveIntervalHour(
                newEquipmentType.BaseMaintenanceIntervalHours ?? 0,
                request.ConditionLevel,
                request.MaintenanceCount);
            equipmentInstance.MaintenanceCount = request.MaintenanceCount;
            equipmentInstance.Note = request.Note;
            equipmentInstance.UpdatedAt = DateTime.Now;

            _unitOfWork.GetRepository<EquipmentInstance>().Update(equipmentInstance);
            _unitOfWork.GetRepository<EquipmentType>().Update(newEquipmentType);

            await _unitOfWork.CommitAsync();

            var updated = await _unitOfWork
                .GetRepository<EquipmentInstance>()
                .FirstOrDefaultAsync(
                    predicate: e => e.EquipmentInstanceId == id,
                    include: query => query
                        .Include(e => e.EquipmentType)
                        .ThenInclude(t => t.EquipmentCategory)
                );

            return MapToResponse(updated!);
        }

        public async Task<bool> DeleteEquipmentInstanceAsync(int id)
        {
            var equipmentInstance = await _unitOfWork
                .GetRepository<EquipmentInstance>()
                .FirstOrDefaultAsync(
                    predicate: e => e.EquipmentInstanceId == id,
                    asNoTracking: false
                );

            if (equipmentInstance == null)
            {
                return false;
            }

            var equipmentType = await _unitOfWork
                .GetRepository<EquipmentType>()
                .FirstOrDefaultAsync(
                    predicate: e => e.EquipmentTypeId == equipmentInstance.EquipmentTypeId,
                    asNoTracking: false
                );

            if (equipmentType != null)
            {
                var status = EnumHelper.ParseEnum<EquipmentInstanceStatus>(equipmentInstance.Status);

                DecreaseEquipmentTypeQuantityByStatus(equipmentType, status);

                _unitOfWork.GetRepository<EquipmentType>().Update(equipmentType);
            }

            _unitOfWork.GetRepository<EquipmentInstance>().Delete(equipmentInstance);

            await _unitOfWork.CommitAsync();

            return true;
        }

        private static EquipmentInstanceResponse MapToResponse(
            EquipmentInstance equipmentInstance)
        {
            return new EquipmentInstanceResponse
            {
                EquipmentInstanceId = equipmentInstance.EquipmentInstanceId,
                EquipmentTypeId = equipmentInstance.EquipmentTypeId,
                EquipmentTypeName = equipmentInstance.EquipmentType?.Name,
                EquipmentCategoryId = equipmentInstance.EquipmentType?.EquipmentCategoryId,
                EquipmentCategoryName = equipmentInstance.EquipmentType?.EquipmentCategory?.CategoryName,
                AssetCode = equipmentInstance.AssetCode,
                SerialNumber = equipmentInstance.SerialNumber,
                TotalUsageHours = equipmentInstance.TotalUsageHour,
                LastMaintenanceDate = equipmentInstance.LastMaintenanceDate,
                UsageHoursSinceMaintenance = equipmentInstance.UsageHoursSinceLastMaintenance,
                ConditionLevel = equipmentInstance.ConditionLevel,
                Status = equipmentInstance.Status,
                EffectiveMaintenanceIntervalHours = equipmentInstance.EffectiveIntervalHour,
                MaintenanceCount = equipmentInstance.MaintenanceCount,
                Note = equipmentInstance.Note,
                CreatedAt = equipmentInstance.CreatedAt,
                UpdatedAt = equipmentInstance.UpdatedAt
            };
        }

        private static void IncreaseEquipmentTypeQuantityByStatus(
            EquipmentType equipmentType,
            EquipmentInstanceStatus status)
        {
            equipmentType.TotalQuantity += 1;

            switch (status)
            {
                case EquipmentInstanceStatus.Available:
                    equipmentType.AvailableQuantity += 1;
                    break;

                case EquipmentInstanceStatus.Reserved:
                    equipmentType.ReservedQuantity += 1;
                    break;

                case EquipmentInstanceStatus.InUse:
                    equipmentType.InUseQuantity += 1;
                    break;

                case EquipmentInstanceStatus.Maintenance:
                case EquipmentInstanceStatus.Damaged:
                    equipmentType.DamagedQuantity += 1;
                    break;

                case EquipmentInstanceStatus.Missing:
                    equipmentType.MissingQuantity += 1;
                    break;
            }

            equipmentType.UpdatedAt = DateTime.Now;
        }

        private static void DecreaseEquipmentTypeQuantityByStatus(
    EquipmentType equipmentType,
    EquipmentInstanceStatus status)
        {
            if (equipmentType.TotalQuantity > 0)
            {
                equipmentType.TotalQuantity -= 1;
            }

            switch (status)
            {
                case EquipmentInstanceStatus.Available:
                    if (equipmentType.AvailableQuantity > 0)
                    {
                        equipmentType.AvailableQuantity -= 1;
                    }
                    break;

                case EquipmentInstanceStatus.Reserved:
                    if (equipmentType.ReservedQuantity > 0)
                    {
                        equipmentType.ReservedQuantity -= 1;
                    }
                    break;

                case EquipmentInstanceStatus.InUse:
                    if (equipmentType.InUseQuantity > 0)
                    {
                        equipmentType.InUseQuantity -= 1;
                    }
                    break;

                case EquipmentInstanceStatus.Maintenance:
                case EquipmentInstanceStatus.Damaged:
                    if (equipmentType.DamagedQuantity > 0)
                    {
                        equipmentType.DamagedQuantity -= 1;
                    }
                    break;

                case EquipmentInstanceStatus.Missing:
                    if (equipmentType.MissingQuantity > 0)
                    {
                        equipmentType.MissingQuantity -= 1;
                    }
                    break;
            }

            equipmentType.UpdatedAt = DateTime.Now;
        }

        public async Task<bool> ReportEquipmentAsync(ReportEquipmentRequest request)
        {
            double totalHours = 0;
            if (request.ReportType == EquipmentInstanceStatus.Returned.ToString())
            {
                var schedules = await _unitOfWork.GetRepository<Schedule>()
                    .GetQueryable()
                    .Where(s => s.AllocationPlanId == request.AllocationPlanId && s.Status == "Completed")
                    .ToListAsync();
                
                foreach (var schedule in schedules)
                {
                    totalHours += (schedule.EndDate - schedule.StartDate).TotalHours;
                }
            }

            foreach (var id in request.EquipmentInstanceIds)
            {
                var eq = await _unitOfWork.GetRepository<EquipmentInstance>().FirstOrDefaultAsync(predicate: e => e.EquipmentInstanceId == id);
                if (eq == null) continue;

                if (request.ReportType == EquipmentInstanceStatus.Returned.ToString())
                {
                    eq.TotalUsageHour += totalHours;
                    eq.UsageHoursSinceLastMaintenance += totalHours;

                    if (eq.EffectiveIntervalHour.HasValue && eq.UsageHoursSinceLastMaintenance >= eq.EffectiveIntervalHour.Value)
                    {
                        eq.Status = EquipmentInstanceStatus.Maintenance.ToString();
                    }
                    else
                    {
                        eq.Status = EquipmentInstanceStatus.Returned.ToString();
                    }
                }
                else if (request.ReportType == EquipmentInstanceStatus.Damaged.ToString())
                {
                    eq.Status = EquipmentInstanceStatus.Damaged.ToString();
                    if (!string.IsNullOrEmpty(request.Note))
                    {
                        eq.Note = string.IsNullOrEmpty(eq.Note) ? $"[Damage Report]: {request.Note}" : eq.Note + $"\n[Damage Report]: {request.Note}";
                    }
                }
                else if (request.ReportType == EquipmentInstanceStatus.Missing.ToString())
                {
                    eq.Status = EquipmentInstanceStatus.Missing.ToString();
                    if (!string.IsNullOrEmpty(request.Note))
                    {
                        eq.Note = string.IsNullOrEmpty(eq.Note) ? $"[Missing Report]: {request.Note}" : eq.Note + $"\n[Missing Report]: {request.Note}";
                    }
                }

                eq.UpdatedAt = DateTime.Now;
                _unitOfWork.GetRepository<EquipmentInstance>().Update(eq);
            }

            await _unitOfWork.CommitAsync();
            return true;
        }

        public async Task<bool> ConfirmReportEquipmentAsync(ConfirmEquipmentRequest request)
        {
            foreach (var id in request.EquipmentInstanceIds)
            {
                var eq = await _unitOfWork.GetRepository<EquipmentInstance>().FirstOrDefaultAsync(predicate: e => e.EquipmentInstanceId == id);
                if (eq == null) continue;

                if (request.ConfirmAction == "AcceptReturned" && eq.Status == EquipmentInstanceStatus.Returned.ToString())
                {
                    eq.Status = EquipmentInstanceStatus.Available.ToString();
                }
                else if (request.ConfirmAction == "SendToMaintenance")
                {
                    eq.Status = EquipmentInstanceStatus.Maintenance.ToString();
                }

                if (!string.IsNullOrEmpty(request.Note))
                {
                    eq.Note = string.IsNullOrEmpty(eq.Note) ? $"[Manager Confirm]: {request.Note}" : eq.Note + $"\n[Manager Confirm]: {request.Note}";
                }

                eq.UpdatedAt = DateTime.Now;
                _unitOfWork.GetRepository<EquipmentInstance>().Update(eq);
            }

            await _unitOfWork.CommitAsync();
            return true;
        }

        private static void ValidateUsageValues(EquipmentInstanceRequest request)
        {
            if (request.TotalUsageHours < 0)
            {
                throw new Exception("Total usage hours cannot be negative.");
            }

            if (request.UsageHoursSinceMaintenance < 0)
            {
                throw new Exception("Usage hours since maintenance cannot be negative.");
            }

            if (request.MaintenanceCount < 0)
            {
                throw new Exception("Maintenance count cannot be negative.");
            }

            if (request.EffectiveMaintenanceIntervalHours.HasValue &&
                request.EffectiveMaintenanceIntervalHours.Value <= 0)
            {
                throw new Exception(
                    "Effective maintenance interval hours must be greater than 0.");
            }
        }

        public static double CalculateEffectiveIntervalHour(double baseInterval, EquipmentConditionLevel condition, int maintenanceCount)
        {
            double conditionFactor = condition switch
            {
                EquipmentConditionLevel.Good => 1.0,
                EquipmentConditionLevel.Fair => 0.85,
                EquipmentConditionLevel.Poor => 0.6,
                EquipmentConditionLevel.Critical => 0.3,
                _ => 1.0
            };

            double maintenanceCountFactor = 1.0;
            if (maintenanceCount >= 3 && maintenanceCount <= 5)
            {
                maintenanceCountFactor = 0.9;
            }
            else if (maintenanceCount >= 6 && maintenanceCount <= 10)
            {
                maintenanceCountFactor = 0.75;
            }
            else if (maintenanceCount > 10)
            {
                maintenanceCountFactor = 0.6;
            }

            return baseInterval * conditionFactor * maintenanceCountFactor;
        }
    }
}
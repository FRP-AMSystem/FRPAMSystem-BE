using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Enums;
using FRPAMSystem.BusinessTier.Payload.EquipmentReturn;
using FRPAMSystem.BusinessTier.Services.Interface;
using FRPAMSystem.DataTier.Abstractions;
using FRPAMSystem.DataTier.Models;
using FRPAMSystem.DataTier.Paginate;
using FRPAMSystem.DataTier.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FRPAMSystem.BusinessTier.Services.Implements
{
    public class EquipmentReturnService : IEquipmentReturnService
    {
        private const string DefaultStatus = "Pending";
        private const string ConfirmedStatus = "Confirmed";

        private readonly IUnitOfWork _unitOfWork;
        private readonly IAllocationEquipmentDetailService _allocationEquipmentDetailService;
        private readonly IClock _clock;

        public EquipmentReturnService(
            IUnitOfWork unitOfWork,
            IAllocationEquipmentDetailService allocationEquipmentDetailService,
            IClock clock)
        {
            _unitOfWork = unitOfWork;
            _allocationEquipmentDetailService = allocationEquipmentDetailService;
            _clock = clock;
        }

        public async Task<IPaginate<EquipmentReturnResponse>> ViewAllAsync(
            EquipmentReturnFilter filter,
            PagingModel pagingModel)
        {
            PagingModelHelper.NormalizePaging(pagingModel);

            var query = _unitOfWork
                .GetRepository<EquipmentReturn>()
                .GetQueryable()
                .ApplyFilter(filter)
                .AsNoTracking()
                .OrderByDescending(r => r.CreatedAt);

            return await query
                .Select(r => new EquipmentReturnResponse
                {
                    ReturnId = r.ReturnId,
                    AllocationEquipmentDetailId = r.AllocationEquipmentDetailId,
                    EquipmentInstanceId = r.EquipmentInstanceId,
                    ReturnedBy = r.ReturnedBy,
                    ReceivedBy = r.ReceivedBy,
                    ReturnDate = r.ReturnDate,
                    Quantity = r.Quantity,
                    ConditionAfter = r.ConditionAfter,
                    IsDamaged = r.IsDamaged,
                    DamageDescription = r.DamageDescription,
                    Note = r.Note,
                    Status = r.Status,
                    ConfirmedAt = r.ConfirmedAt,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt
                })
                .ToPaginateAsync(pagingModel.Page, pagingModel.Size, 1);
        }

        public async Task<EquipmentReturnResponse?> GetByIdAsync(int id)
        {
            var equipmentReturn = await _unitOfWork
                .GetRepository<EquipmentReturn>()
                .FirstOrDefaultAsync(predicate: r => r.ReturnId == id);

            return equipmentReturn == null ? null : MapToResponse(equipmentReturn);
        }

        public async Task<EquipmentReturnResponse?> SubmitMineAsync(
            int allocationEquipmentDetailId,
            int userId,
            EquipmentReturnMineRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ConditionAfter))
            {
                throw new Exception("Condition after return is required.");
            }

            if (request.IsDamaged && string.IsNullOrWhiteSpace(request.DamageDescription))
            {
                throw new Exception("Damage description is required when equipment is marked as damaged.");
            }

            if (!await _allocationEquipmentDetailService
                    .UserCanAccessAllocationEquipmentDetailAsync(allocationEquipmentDetailId, userId))
            {
                return null;
            }

            var detail = await _unitOfWork
                .GetRepository<AllocationEquipmentDetail>()
                .FirstOrDefaultAsync(
                    predicate: d => d.AllocationEquipmentDetailId == allocationEquipmentDetailId,
                    include: query => query.Include(d => d.EquipmentInstance),
                    asNoTracking: false
                );

            if (detail == null)
            {
                return null;
            }

            if (!IsReturnEligible(detail.Status))
            {
                throw new Exception("Equipment must be in InUse status before return.");
            }

            var now = _clock.Now;

            var equipmentReturn = new EquipmentReturn
            {
                AllocationEquipmentDetailId = detail.AllocationEquipmentDetailId,
                EquipmentInstanceId = detail.EquipmentInstanceId,
                ReturnedBy = userId,
                ReceivedBy = userId,
                ReturnDate = now,
                Quantity = detail.Quantity,
                ConditionAfter = request.ConditionAfter.Trim(),
                IsDamaged = request.IsDamaged,
                DamageDescription = request.DamageDescription,
                Note = request.Note,
                Status = ConfirmedStatus,
                ConfirmedAt = now
            };

            detail.Status = AllocationDetailStatus.Completed.ToString();

            if (detail.EquipmentInstanceId.HasValue && detail.EquipmentInstance != null)
            {
                detail.EquipmentInstance.Status = EquipmentInstanceStatus.Available.ToString();
                _unitOfWork.GetRepository<EquipmentInstance>().Update(detail.EquipmentInstance);
            }

            await _unitOfWork.GetRepository<EquipmentReturn>().InsertAsync(equipmentReturn);
            _unitOfWork.GetRepository<AllocationEquipmentDetail>().Update(detail);
            await _unitOfWork.CommitAsync();

            return MapToResponse(equipmentReturn);
        }

        public async Task<EquipmentReturnResponse> CreateAsync(EquipmentReturnRequest request)
        {
            await ValidateRequestAsync(request);

            var equipmentReturn = new EquipmentReturn
            {
                AllocationEquipmentDetailId = request.AllocationEquipmentDetailId,
                EquipmentInstanceId = request.EquipmentInstanceId,
                ReturnedBy = request.ReturnedBy,
                ReceivedBy = request.ReceivedBy,
                ReturnDate = request.ReturnDate,
                Quantity = request.Quantity,
                ConditionAfter = request.ConditionAfter.Trim(),
                IsDamaged = request.IsDamaged,
                DamageDescription = request.DamageDescription,
                Note = request.Note,
                Status = string.IsNullOrWhiteSpace(request.Status) ? DefaultStatus : request.Status.Trim(),
                ConfirmedAt = request.ConfirmedAt
            };

            await _unitOfWork.GetRepository<EquipmentReturn>().InsertAsync(equipmentReturn);
            await _unitOfWork.CommitAsync();

            return MapToResponse(equipmentReturn);
        }

        public async Task<EquipmentReturnResponse?> UpdateAsync(
            int id,
            EquipmentReturnRequest request)
        {
            await ValidateRequestAsync(request);

            var equipmentReturn = await _unitOfWork
                .GetRepository<EquipmentReturn>()
                .FirstOrDefaultAsync(
                    predicate: r => r.ReturnId == id,
                    asNoTracking: false
                );

            if (equipmentReturn == null)
            {
                return null;
            }

            equipmentReturn.AllocationEquipmentDetailId = request.AllocationEquipmentDetailId;
            equipmentReturn.EquipmentInstanceId = request.EquipmentInstanceId;
            equipmentReturn.ReturnedBy = request.ReturnedBy;
            equipmentReturn.ReceivedBy = request.ReceivedBy;
            equipmentReturn.ReturnDate = request.ReturnDate;
            equipmentReturn.Quantity = request.Quantity;
            equipmentReturn.ConditionAfter = request.ConditionAfter.Trim();
            equipmentReturn.IsDamaged = request.IsDamaged;
            equipmentReturn.DamageDescription = request.DamageDescription;
            equipmentReturn.Note = request.Note;
            equipmentReturn.Status = string.IsNullOrWhiteSpace(request.Status)
                ? equipmentReturn.Status
                : request.Status.Trim();
            equipmentReturn.ConfirmedAt = request.ConfirmedAt;

            _unitOfWork.GetRepository<EquipmentReturn>().Update(equipmentReturn);
            await _unitOfWork.CommitAsync();

            return MapToResponse(equipmentReturn);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var equipmentReturn = await _unitOfWork
                .GetRepository<EquipmentReturn>()
                .FirstOrDefaultAsync(
                    predicate: r => r.ReturnId == id,
                    asNoTracking: false
                );

            if (equipmentReturn == null)
            {
                return false;
            }

            _unitOfWork.GetRepository<EquipmentReturn>().Delete(equipmentReturn);
            await _unitOfWork.CommitAsync();

            return true;
        }

        private async Task ValidateRequestAsync(EquipmentReturnRequest request)
        {
            if (request.Quantity <= 0)
            {
                throw new Exception("Quantity must be greater than 0.");
            }

            if (string.IsNullOrWhiteSpace(request.ConditionAfter))
            {
                throw new Exception("Condition after return is required.");
            }

            if (request.IsDamaged && string.IsNullOrWhiteSpace(request.DamageDescription))
            {
                throw new Exception("Damage description is required when equipment is marked as damaged.");
            }

            var detailExists = await _unitOfWork
                .GetRepository<AllocationEquipmentDetail>()
                .AnyAsync(d => d.AllocationEquipmentDetailId == request.AllocationEquipmentDetailId);

            if (!detailExists)
            {
                throw new Exception("Allocation equipment detail does not exist.");
            }

            if (request.EquipmentInstanceId.HasValue)
            {
                var instanceExists = await _unitOfWork
                    .GetRepository<EquipmentInstance>()
                    .AnyAsync(e => e.EquipmentInstanceId == request.EquipmentInstanceId.Value);

                if (!instanceExists)
                {
                    throw new Exception("Equipment instance does not exist.");
                }
            }

            await EnsureUserExistsAsync(request.ReturnedBy, "Returned-by user");
            await EnsureUserExistsAsync(request.ReceivedBy, "Receiving user");
        }

        private async Task EnsureUserExistsAsync(int userId, string label)
        {
            var exists = await _unitOfWork
                .GetRepository<User>()
                .AnyAsync(u => u.UserId == userId);

            if (!exists)
            {
                throw new Exception($"{label} does not exist.");
            }
        }

        private static EquipmentReturnResponse MapToResponse(EquipmentReturn equipmentReturn)
        {
            return new EquipmentReturnResponse
            {
                ReturnId = equipmentReturn.ReturnId,
                AllocationEquipmentDetailId = equipmentReturn.AllocationEquipmentDetailId,
                EquipmentInstanceId = equipmentReturn.EquipmentInstanceId,
                ReturnedBy = equipmentReturn.ReturnedBy,
                ReceivedBy = equipmentReturn.ReceivedBy,
                ReturnDate = equipmentReturn.ReturnDate,
                Quantity = equipmentReturn.Quantity,
                ConditionAfter = equipmentReturn.ConditionAfter,
                IsDamaged = equipmentReturn.IsDamaged,
                DamageDescription = equipmentReturn.DamageDescription,
                Note = equipmentReturn.Note,
                Status = equipmentReturn.Status,
                ConfirmedAt = equipmentReturn.ConfirmedAt,
                CreatedAt = equipmentReturn.CreatedAt,
                UpdatedAt = equipmentReturn.UpdatedAt
            };
        }

        private static bool IsReturnEligible(string status)
        {
            return status == AllocationDetailStatus.InUse.ToString() ||
                string.Equals(status, "Active", StringComparison.OrdinalIgnoreCase);
        }
    }
}

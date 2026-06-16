using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.EquipmentShortageLog;
using FRPAMSystem.BusinessTier.Services.Interface;
using FRPAMSystem.DataTier.Models;
using FRPAMSystem.DataTier.Paginate;
using FRPAMSystem.DataTier.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FRPAMSystem.BusinessTier.Services.Implements
{
    public class EquipmentShortageLogService : IEquipmentShortageLogService
    {
        private readonly IUnitOfWork _unitOfWork;

        public EquipmentShortageLogService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IPaginate<EquipmentShortageLogResponse>> ViewAllAsync(
            EquipmentShortageLogFilter filter,
            PagingModel pagingModel)
        {
            PagingModelHelper.NormalizePaging(pagingModel);

            var query = _unitOfWork
                .GetRepository<EquipmentShortageLog>()
                .GetQueryable()
                .ApplyFilter(filter)
                .AsNoTracking()
                .OrderByDescending(r => r.CreatedAt);

            return await query
                .Select(r => new EquipmentShortageLogResponse
                {
                    ShortageLogId = r.ShortageLogId,
                    AllocationPlanId = r.AllocationPlanId,
                    ExpEquipmentReqId = r.ExpEquipmentReqId,
                    PhaseEquipmentReqId = r.PhaseEquipmentReqId,
                    ShortageQuantity = r.ShortageQuantity,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt
                })
                .ToPaginateAsync(pagingModel.Page, pagingModel.Size, 1);
        }

        public async Task<EquipmentShortageLogResponse?> GetByIdAsync(int id)
        {
            var log = await _unitOfWork
                .GetRepository<EquipmentShortageLog>()
                .FirstOrDefaultAsync(predicate: r => r.ShortageLogId == id);

            return log == null ? null : MapToResponse(log);
        }

        public async Task<EquipmentShortageLogResponse> CreateAsync(EquipmentShortageLogRequest request)
        {
            await ValidateRequestAsync(request);

            var log = new EquipmentShortageLog
            {
                AllocationPlanId = request.AllocationPlanId,
                ExpEquipmentReqId = request.ExpEquipmentReqId,
                PhaseEquipmentReqId = request.PhaseEquipmentReqId,
                ShortageQuantity = request.ShortageQuantity,
                CreatedAt = DateTime.Now
            };

            await _unitOfWork.GetRepository<EquipmentShortageLog>().InsertAsync(log);
            await _unitOfWork.CommitAsync();

            return MapToResponse(log);
        }

        public async Task<EquipmentShortageLogResponse?> UpdateAsync(
            int id,
            EquipmentShortageLogRequest request)
        {
            await ValidateRequestAsync(request);

            var log = await _unitOfWork
                .GetRepository<EquipmentShortageLog>()
                .FirstOrDefaultAsync(
                    predicate: r => r.ShortageLogId == id,
                    asNoTracking: false
                );

            if (log == null)
            {
                return null;
            }

            log.AllocationPlanId = request.AllocationPlanId;
            log.ExpEquipmentReqId = request.ExpEquipmentReqId;
            log.PhaseEquipmentReqId = request.PhaseEquipmentReqId;
            log.ShortageQuantity = request.ShortageQuantity;
            log.UpdatedAt = DateTime.Now;

            _unitOfWork.GetRepository<EquipmentShortageLog>().Update(log);
            await _unitOfWork.CommitAsync();

            return MapToResponse(log);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var log = await _unitOfWork
                .GetRepository<EquipmentShortageLog>()
                .FirstOrDefaultAsync(
                    predicate: r => r.ShortageLogId == id,
                    asNoTracking: false
                );

            if (log == null)
            {
                return false;
            }

            _unitOfWork.GetRepository<EquipmentShortageLog>().Delete(log);
            await _unitOfWork.CommitAsync();

            return true;
        }

        private async Task ValidateRequestAsync(EquipmentShortageLogRequest request)
        {
            if (request.ShortageQuantity <= 0)
            {
                throw new Exception("Shortage quantity must be greater than 0.");
            }

            var hasExpReq = request.ExpEquipmentReqId.HasValue;
            var hasPhaseReq = request.PhaseEquipmentReqId.HasValue;

            if (hasExpReq == hasPhaseReq)
            {
                throw new Exception("Exactly one of experiment equipment requirement or phase equipment requirement must be provided.");
            }

            var allocationPlanExists = await _unitOfWork
                .GetRepository<AllocationPlan>()
                .AnyAsync(p => p.AllocationPlanId == request.AllocationPlanId);

            if (!allocationPlanExists)
            {
                throw new Exception("Allocation plan does not exist.");
            }

            if (hasExpReq)
            {
                var expReqExists = await _unitOfWork
                    .GetRepository<ExperimentEquipmentRequirement>()
                    .AnyAsync(r => r.ExpEquipmentReqId == request.ExpEquipmentReqId!.Value);

                if (!expReqExists)
                {
                    throw new Exception("Experiment equipment requirement does not exist.");
                }
            }

            if (hasPhaseReq)
            {
                var phaseReqExists = await _unitOfWork
                    .GetRepository<PhaseEquipmentRequirement>()
                    .AnyAsync(r => r.PhaseEquipmentReqId == request.PhaseEquipmentReqId!.Value);

                if (!phaseReqExists)
                {
                    throw new Exception("Phase equipment requirement does not exist.");
                }
            }
        }

        private static EquipmentShortageLogResponse MapToResponse(EquipmentShortageLog log)
        {
            return new EquipmentShortageLogResponse
            {
                ShortageLogId = log.ShortageLogId,
                AllocationPlanId = log.AllocationPlanId,
                ExpEquipmentReqId = log.ExpEquipmentReqId,
                PhaseEquipmentReqId = log.PhaseEquipmentReqId,
                ShortageQuantity = log.ShortageQuantity,
                CreatedAt = log.CreatedAt,
                UpdatedAt = log.UpdatedAt
            };
        }
    }
}

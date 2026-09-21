using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Enums;
using FRPAMSystem.BusinessTier.Payload.LandResource;
using FRPAMSystem.BusinessTier.Services.Interface;
using FRPAMSystem.DataTier.Abstractions;
using FRPAMSystem.DataTier.Models;
using FRPAMSystem.DataTier.Paginate;
using FRPAMSystem.DataTier.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FRPAMSystem.BusinessTier.Services.Implements
{
    public class LandResourceService : ILandResourceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClock? _clock;

        public LandResourceService(IUnitOfWork unitOfWork, IClock? clock = null)
        {
            _unitOfWork = unitOfWork;
            _clock = clock;
        }

        public async Task<IPaginate<LandResourceResponse>> ViewAllLandResourcesAsync(
            LandResourceFilter filter,
            PagingModel pagingModel)
        {
            PagingModelHelper.NormalizePaging(pagingModel);

            var query = _unitOfWork
                .GetRepository<LandResource>()
                .GetQueryable()
                .Include(l => l.Area)
                .ApplyFilter(filter)
                .AsNoTracking()
                .OrderBy(l => l.LandCode);

            return await query
                .Select(l => new LandResourceResponse
                {
                    LandId = l.LandId,
                    AreaId = l.AreaId,
                    AreaName = l.Area.AreaName,
                    LandCode = l.LandCode,
                    AreaSize = l.AreaSize,
                    Location = l.Location,
                    SoilType = l.SoilType,
                    Status = l.Status,
                    CreatedAt = l.CreatedAt,
                    UpdatedAt = l.UpdatedAt
                })
                .ToPaginateAsync(pagingModel.Page, pagingModel.Size, 1);
        }

        public async Task<LandResourceResponse?> GetLandResourceByIdAsync(int id)
        {
            await SyncLandStatusAsync(id);

            var landResource = await _unitOfWork
                .GetRepository<LandResource>()
                .FirstOrDefaultAsync(
                    predicate: l => l.LandId == id,
                    include: query => query.Include(l => l.Area)
                );

            if (landResource == null)
            {
                return null;
            }

            return MapToResponse(landResource);
        }

        public async Task<LandResourceResponse> CreateLandResourceAsync(LandResourceRequest request)
        {
            await ValidateRequestAsync(request);

            var landResource = new LandResource
            {
                AreaId = request.AreaId,
                LandCode = request.LandCode.Trim(),
                AreaSize = request.AreaSize,
                Location = request.Location,
                SoilType = request.SoilType.Trim(),
                Status = request.Status.ToString()
            };

            await _unitOfWork.GetRepository<LandResource>().InsertAsync(landResource);
            await _unitOfWork.CommitAsync();

            if (request.Status != LandResourceStatus.Unavailable)
            {
                await SyncLandStatusAsync(landResource.LandId);
            }

            return (await GetLandResourceByIdAsync(landResource.LandId))!;
        }

        public async Task<LandResourceResponse?> UpdateLandResourceAsync(
            int id,
            LandResourceRequest request)
        {
            await ValidateRequestAsync(request, id);

            var landResource = await _unitOfWork
                .GetRepository<LandResource>()
                .FirstOrDefaultAsync(
                    predicate: l => l.LandId == id,
                    asNoTracking: false
                );

            if (landResource == null)
            {
                return null;
            }

            landResource.AreaId = request.AreaId;
            landResource.LandCode = request.LandCode.Trim();
            landResource.AreaSize = request.AreaSize;
            landResource.Location = request.Location;
            landResource.SoilType = request.SoilType.Trim();
            landResource.Status = request.Status.ToString();
            landResource.UpdatedAt = _clock?.Now ?? DateTime.UtcNow;

            _unitOfWork.GetRepository<LandResource>().Update(landResource);
            await _unitOfWork.CommitAsync();

            if (request.Status != LandResourceStatus.Unavailable)
            {
                await SyncLandStatusAsync(id);
            }

            return await GetLandResourceByIdAsync(id);
        }

        public async Task<bool> DeleteLandResourceAsync(int id)
        {
            var landResource = await _unitOfWork
                .GetRepository<LandResource>()
                .FirstOrDefaultAsync(
                    predicate: l => l.LandId == id,
                    asNoTracking: false
                );

            if (landResource == null)
            {
                return false;
            }

            _unitOfWork.GetRepository<LandResource>().Delete(landResource);
            await _unitOfWork.CommitAsync();

            return true;
        }

        public async Task SyncLandStatusesAsync(IEnumerable<int> landIds)
        {
            var distinctIds = landIds.Distinct().ToList();
            foreach (var id in distinctIds)
            {
                await SyncLandStatusAsync(id);
            }
        }

        public async Task SyncLandStatusAsync(int landId)
        {
            var landResource = await _unitOfWork
                .GetRepository<LandResource>()
                .FirstOrDefaultAsync(
                    predicate: l => l.LandId == landId,
                    asNoTracking: false
                );

            if (landResource == null)
            {
                return;
            }

            // Do not overwrite administrative Unavailable status
            if (string.Equals(landResource.Status, LandResourceStatus.Unavailable.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var now = _clock?.Now ?? DateTime.UtcNow;
            var today = now.Date;
            var cancelledDetailStatus = AllocationDetailStatus.Cancelled.ToString();
            var completedDetailStatus = AllocationDetailStatus.Completed.ToString();
            var approvedPlanStatus = AllocationPlanStatus.Approved.ToString();

            var activeAllocations = await _unitOfWork
                .GetRepository<AllocationLandDetail>()
                .GetQueryable()
                .Include(d => d.AllocationPlan)
                .Where(d => d.LandId == landId &&
                            d.Status != cancelledDetailStatus &&
                            d.Status != completedDetailStatus &&
                            d.AllocationPlan.ApproveStatus == approvedPlanStatus)
                .ToListAsync();

            string targetStatus;
            if (activeAllocations.Any(d => d.Status == AllocationDetailStatus.InUse.ToString() ||
                                           (d.StartDate.Date <= today && today <= d.EndDate.Date)))
            {
                targetStatus = LandResourceStatus.InUse.ToString();
            }
            else if (activeAllocations.Any(d => d.EndDate.Date >= today))
            {
                targetStatus = LandResourceStatus.Reserved.ToString();
            }
            else
            {
                targetStatus = LandResourceStatus.Available.ToString();
            }

            if (!string.Equals(landResource.Status, targetStatus, StringComparison.OrdinalIgnoreCase))
            {
                landResource.Status = targetStatus;
                landResource.UpdatedAt = now;
                _unitOfWork.GetRepository<LandResource>().Update(landResource);
                await _unitOfWork.CommitAsync();
            }
        }

        private async Task ValidateRequestAsync(LandResourceRequest request, int? excludeId = null)
        {
            if (!Enum.IsDefined(typeof(LandResourceStatus), request.Status))
            {
                throw new Exception($"Invalid land resource status '{request.Status}'.");
            }

            if (string.IsNullOrWhiteSpace(request.LandCode))
            {
                throw new Exception("Land code is required.");
            }

            if (string.IsNullOrWhiteSpace(request.SoilType))
            {
                throw new Exception("Soil type is required.");
            }

            if (request.AreaSize <= 0)
            {
                throw new Exception("Area size must be greater than 0.");
            }

            var areaExists = await _unitOfWork
                .GetRepository<Area>()
                .AnyAsync(a => a.AreaId == request.AreaId);

            if (!areaExists)
            {
                throw new Exception("Area does not exist.");
            }

            var duplicateCodeQuery = _unitOfWork
                .GetRepository<LandResource>()
                .GetQueryable()
                .Where(l => l.LandCode == request.LandCode.Trim());

            if (excludeId.HasValue)
            {
                duplicateCodeQuery = duplicateCodeQuery.Where(l => l.LandId != excludeId.Value);
            }

            if (await duplicateCodeQuery.AnyAsync())
            {
                throw new Exception("Land code already exists.");
            }

            if (excludeId.HasValue && request.Status == LandResourceStatus.Unavailable)
            {
                var today = (_clock?.Now ?? DateTime.UtcNow).Date;
                var inUseStatus = AllocationDetailStatus.InUse.ToString();
                var allocatedStatus = AllocationDetailStatus.Allocated.ToString();
                var approvedPlanStatus = AllocationPlanStatus.Approved.ToString();

                var hasActiveAllocation = await _unitOfWork
                    .GetRepository<AllocationLandDetail>()
                    .GetQueryable()
                    .Include(d => d.AllocationPlan)
                    .AnyAsync(d => d.LandId == excludeId.Value &&
                                   d.AllocationPlan.ApproveStatus == approvedPlanStatus &&
                                   (d.Status == inUseStatus ||
                                    (d.Status == allocatedStatus && d.StartDate.Date <= today && today <= d.EndDate.Date)));

                if (hasActiveAllocation)
                {
                    throw new Exception("Cannot set land to Unavailable while it is actively in use by an approved allocation.");
                }
            }
        }

        private static LandResourceResponse MapToResponse(LandResource landResource)
        {
            return new LandResourceResponse
            {
                LandId = landResource.LandId,
                AreaId = landResource.AreaId,
                AreaName = landResource.Area.AreaName,
                LandCode = landResource.LandCode,
                AreaSize = landResource.AreaSize,
                Location = landResource.Location,
                SoilType = landResource.SoilType,
                Status = landResource.Status,
                CreatedAt = landResource.CreatedAt,
                UpdatedAt = landResource.UpdatedAt
            };
        }
    }
}

using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.Area;
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
    public class AreaService : IAreaService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AreaService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IPaginate<AreaResponse>> ViewAllAreasAsync(
            AreaFilter filter,
            PagingModel pagingModel)
        {
            PagingModelHelper.NormalizePaging(pagingModel);

            var query = _unitOfWork
                .GetRepository<Area>()
                .GetQueryable()
                .ApplyFilter(filter)
                .AsNoTracking()
                .OrderBy(a => a.AreaName);

            return await query
                .Select(a => new AreaResponse
                {
                    AreaId = a.AreaId,
                    AreaName = a.AreaName,
                    Description = a.Description,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt
                })
                .ToPaginateAsync(pagingModel.Page, pagingModel.Size, 1);
        } 

        public async Task<AreaResponse?> GetAreaByIdAsync(int id)
        {
            var area = await _unitOfWork
                .GetRepository<Area>()
                .FirstOrDefaultAsync(predicate: a => a.AreaId == id);

            if (area == null)
            {
                return null;
            }

            return new AreaResponse
            {
                AreaId = area.AreaId,
                AreaName = area.AreaName,
                Description = area.Description,
                CreatedAt = area.CreatedAt,
                UpdatedAt = area.UpdatedAt
            };
        }

        public async Task<AreaResponse> CreateAreaAsync(AreaRequest request)
        {
            var exists = await _unitOfWork
                .GetRepository<Area>()
                .AnyAsync(a => a.AreaName == request.AreaName);

            if (exists)
            {
                throw new Exception("Area name already exists.");
            }

            var area = new Area
            {
                AreaName = request.AreaName,
                Description = request.Description,
                CreatedAt = DateTime.Now
            };

            await _unitOfWork.GetRepository<Area>().InsertAsync(area);
            await _unitOfWork.CommitAsync();

            return new AreaResponse
            {
                AreaId = area.AreaId,
                AreaName = area.AreaName,
                Description = area.Description,
                CreatedAt = area.CreatedAt,
                UpdatedAt = area.UpdatedAt
            };
        }

        public async Task<AreaResponse?> UpdateAreaAsync(int id, AreaRequest request)
        {
            var area = await _unitOfWork
                .GetRepository<Area>()
                .FirstOrDefaultAsync(
                    predicate: a => a.AreaId == id,
                    asNoTracking: false
                );

            if (area == null)
            {
                return null;
            }

            var duplicateName = await _unitOfWork
                .GetRepository<Area>()
                .AnyAsync(a => a.AreaName == request.AreaName && a.AreaId != id);

            if (duplicateName)
            {
                throw new Exception("Area name already exists.");
            }

            area.AreaName = request.AreaName;
            area.Description = request.Description;
            area.UpdatedAt = DateTime.Now;

            _unitOfWork.GetRepository<Area>().Update(area);
            await _unitOfWork.CommitAsync();

            return new AreaResponse
            {
                AreaId = area.AreaId,
                AreaName = area.AreaName,
                Description = area.Description,
                CreatedAt = area.CreatedAt,
                UpdatedAt = area.UpdatedAt
            };
        }

        public async Task<bool> DeleteAreaAsync(int id)
        {
            var area = await _unitOfWork
                .GetRepository<Area>()
                .FirstOrDefaultAsync(
                    predicate: a => a.AreaId == id,
                    asNoTracking: false
                );

            if (area == null)
            {
                return false;
            }

            _unitOfWork.GetRepository<Area>().Delete(area);
            await _unitOfWork.CommitAsync();

            return true;
        }
    }
}

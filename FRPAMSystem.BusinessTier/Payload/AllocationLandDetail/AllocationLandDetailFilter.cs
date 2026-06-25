using FRPAMSystem.BusinessTier.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.Payload.AllocationLandDetail
{
    public class AllocationLandDetailFilter
    {
        public string? Keyword { get; set; }

        public int? AllocationPlanId { get; set; }

        public int? ExperimentId { get; set; }

        public int? LandId { get; set; }

        public int? AreaId { get; set; }

        public int? ExpLandReqId { get; set; }

        public AllocationDetailStatus? Status { get; set; }

        public DateTime? StartFrom { get; set; }

        public DateTime? StartTo { get; set; }

        public DateTime? EndFrom { get; set; }

        public DateTime? EndTo { get; set; }
    }
}

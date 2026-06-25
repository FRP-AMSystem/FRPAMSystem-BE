using FRPAMSystem.BusinessTier.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.Payload.AllocationEquipmentDetail
{
    public class AllocationEquipmentDetailFilter
    {
        public string? Keyword { get; set; }

        public int? AllocationPlanId { get; set; }

        public int? ExperimentId { get; set; }

        public int? ExpEquipmentReqId { get; set; }

        public int? PhaseEquipmentReqId { get; set; }

        public int? AllocatedEquipmentTypeId { get; set; }

        public int? EquipmentInstanceId { get; set; }

        public bool? IsSubstitute { get; set; }

        public AllocationDetailStatus? Status { get; set; }

        public DateTime? StartFrom { get; set; }

        public DateTime? StartTo { get; set; }

        public DateTime? EndFrom { get; set; }

        public DateTime? EndTo { get; set; }
    }
}

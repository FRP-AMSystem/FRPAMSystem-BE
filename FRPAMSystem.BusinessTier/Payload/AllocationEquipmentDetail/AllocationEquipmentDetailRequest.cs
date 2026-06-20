using FRPAMSystem.BusinessTier.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.Payload.AllocationEquipmentDetail
{
    public class AllocationEquipmentDetailRequest
    {
        public int AllocationPlanId { get; set; }

        public int? ExpEquipmentReqId { get; set; }

        public int? PhaseEquipmentReqId { get; set; }

        public int AllocatedEquipmentTypeId { get; set; }

        public int? EquipmentInstanceId { get; set; }

        public int Quantity { get; set; }

        public bool IsSubstitute { get; set; }

        public double EfficiencyRate { get; set; } = 1;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public AllocationDetailStatus Status { get; set; } = AllocationDetailStatus.Proposed;
    }
}

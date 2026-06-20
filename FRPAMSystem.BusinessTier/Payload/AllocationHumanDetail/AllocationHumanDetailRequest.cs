using FRPAMSystem.BusinessTier.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.Payload.AllocationHumanDetail
{
    public class AllocationHumanDetailRequest
    {
        public int AllocationPlanId { get; set; }

        public int? ExpHumanReqId { get; set; }

        public int? PhaseHumanReqId { get; set; }

        public int HumanResourceId { get; set; }

        public double WorkingHours { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public AllocationDetailStatus Status { get; set; } = AllocationDetailStatus.Proposed;
    }
}

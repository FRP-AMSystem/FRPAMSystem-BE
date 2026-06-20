using FRPAMSystem.BusinessTier.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.Payload.AllocationLandDetail
{
    public class AllocationLandDetailRequest
    {
        public int AllocationPlanId { get; set; }

        public int LandId { get; set; }

        public int ExpLandReqId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public AllocationDetailStatus Status { get; set; } = AllocationDetailStatus.Proposed;
    }
}

using FRPAMSystem.BusinessTier.Enums;
using FRPAMSystem.BusinessTier.Payload.AllocationEquipmentDetail;
using FRPAMSystem.BusinessTier.Payload.AllocationHumanDetail;
using FRPAMSystem.BusinessTier.Payload.AllocationLandDetail;
using System.Collections.Generic;

namespace FRPAMSystem.BusinessTier.Payload.AllocationPlan
{
    public class CreateAllocationPlanWithDetailsRequest
    {
        public int ExperimentId { get; set; }

        public AllocationPlanStatus ApproveStatus { get; set; } = AllocationPlanStatus.Draft;

        public List<AllocationLandDetailRequest> LandDetails { get; set; } = new();

        public List<AllocationEquipmentDetailRequest> EquipmentDetails { get; set; } = new();

        public List<AllocationHumanDetailRequest> HumanDetails { get; set; } = new();
    }
}

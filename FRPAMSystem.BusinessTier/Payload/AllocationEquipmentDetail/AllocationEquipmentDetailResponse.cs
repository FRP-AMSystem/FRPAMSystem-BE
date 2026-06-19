using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.Payload.AllocationEquipmentDetail
{
    public class AllocationEquipmentDetailResponse
    {
        public int AllocationEquipmentDetailId { get; set; }

        public int AllocationPlanId { get; set; }

        public int ExperimentId { get; set; }

        public string? ExperimentName { get; set; }

        public int? ExpEquipmentReqId { get; set; }

        public int? PhaseEquipmentReqId { get; set; }

        public int? PhaseId { get; set; }

        public string? PhaseName { get; set; }

        public int? RequestedEquipmentTypeId { get; set; }

        public string? RequestedEquipmentTypeName { get; set; }

        public int AllocatedEquipmentTypeId { get; set; }

        public string? AllocatedEquipmentTypeName { get; set; }

        public string? TrackingType { get; set; }

        public int? EquipmentInstanceId { get; set; }

        public string? AssetCode { get; set; }

        public string? SerialNumber { get; set; }

        public int Quantity { get; set; }

        public bool IsSubstitute { get; set; }

        public double EfficiencyRate { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}

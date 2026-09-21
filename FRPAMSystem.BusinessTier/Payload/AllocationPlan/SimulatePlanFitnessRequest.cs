using FRPAMSystem.BusinessTier.Enums;
using System;
using System.Collections.Generic;

namespace FRPAMSystem.BusinessTier.Payload.AllocationPlan
{
    public class SimulatePlanFitnessRequest
    {
        public int ExperimentId { get; set; }

        public int? CurrentPlanId { get; set; }

        public List<SimulateLandDetailItem> LandDetails { get; set; } = new();

        public List<SimulateEquipmentDetailItem> EquipmentDetails { get; set; } = new();

        public List<SimulateHumanDetailItem> HumanDetails { get; set; } = new();
    }

    public class SimulateLandDetailItem
    {
        public int LandId { get; set; }

        public double AllocatedArea { get; set; }

        public int? ExpLandReqId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }

    public class SimulateEquipmentDetailItem
    {
        public int? EquipmentTypeId { get; set; }

        public int? EquipmentInstanceId { get; set; }

        public int Quantity { get; set; } = 1;

        public int? ExpEquipmentReqId { get; set; }

        public int? PhaseEquipmentReqId { get; set; }

        public bool IsSubstitute { get; set; }

        public double EfficiencyRate { get; set; } = 1.0;

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }

    public class SimulateHumanDetailItem
    {
        public int HumanResourceId { get; set; }

        public string? AssignedRole { get; set; }

        public int? ExpHumanReqId { get; set; }

        public int? PhaseHumanReqId { get; set; }

        public double WorkingHours { get; set; } = 8.0;

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }
}

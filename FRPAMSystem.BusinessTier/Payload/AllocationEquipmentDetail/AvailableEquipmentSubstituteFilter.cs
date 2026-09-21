using System;
using System.ComponentModel.DataAnnotations;

namespace FRPAMSystem.BusinessTier.Payload.AllocationEquipmentDetail
{
    public class AvailableEquipmentSubstituteFilter
    {
        [Required]
        public int EquipmentTypeId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public double? MinAcceptableEfficiency { get; set; }

        public bool IncludePrimary { get; set; } = true;

        public int? CurrentAllocationDetailId { get; set; }
    }
}

using System;
using System.ComponentModel.DataAnnotations;

namespace FRPAMSystem.BusinessTier.Payload.AllocationHumanDetail
{
    public class AvailableHumanFilter
    {
        [Required]
        public int RoleId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public int? RequiredSkillId { get; set; }

        public double RequestedHoursPerDay { get; set; } = 8.0;

        public int? CurrentAllocationDetailId { get; set; }
    }
}

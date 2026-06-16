using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.Payload.HumanResourceProfile
{
    public class HumanResourceProfileResponse
    {
        public int HumanResourceId { get; set; }

        public int UserId { get; set; }

        public string? FullName { get; set; }

        public string? Username { get; set; }

        public string? Email { get; set; }

        public int? RoleId { get; set; }

        public string? RoleName { get; set; }

        public double MaxWorkingHoursPerDay { get; set; }

        public double CurrentWorkload { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}

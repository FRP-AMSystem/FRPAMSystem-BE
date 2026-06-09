using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.Payload.Area
{
    public class AreaRequest
    {
        public string AreaName { get; set; } = string.Empty;

        public string? Description { get; set; }
    }
}

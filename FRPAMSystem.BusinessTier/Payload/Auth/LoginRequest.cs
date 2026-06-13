using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.Payload.Auth
{
    public class LoginRequest
    {
        public string UsernameOrEmail { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }
}

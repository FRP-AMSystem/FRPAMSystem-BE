using System;
using System.Collections.Generic;
using System.Text;

namespace FRPAMSystem.BusinessTier.Utils
{
    public static class PasswordUtil
    {
        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public static bool VerifyPassword(string password, string passwordHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
    }
}

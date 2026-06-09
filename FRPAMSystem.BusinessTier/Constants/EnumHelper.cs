using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.Constants
{
    public static class EnumHelper
    {
        public static TEnum ParseEnum<TEnum>(string value)
     where TEnum : struct, Enum
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new Exception($"{typeof(TEnum).Name} value cannot be empty.");
            }

            value = value.Trim();

            if (!Enum.TryParse<TEnum>(
                    value,
                    ignoreCase: true,
                    out var result))
            {
                throw new Exception(
                    $"Invalid {typeof(TEnum).Name} value: {value}");
            }

            return result;
        }

        public static string ToEnumString<TEnum>(TEnum value)
            where TEnum : struct, Enum
        {
            return value.ToString();
        }

        public static bool IsValidEnum<TEnum>(string value)
            where TEnum : struct, Enum
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            return Enum.TryParse<TEnum>(
                value,
                ignoreCase: true,
                out _);
        }
    }
}

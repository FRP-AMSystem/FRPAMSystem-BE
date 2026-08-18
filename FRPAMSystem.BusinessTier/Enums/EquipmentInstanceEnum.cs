using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.Enums
{
    public enum EquipmentInstanceStatus
    {
        Available,
        Reserved,
        InUse,
        Maintenance,
        Damaged,
        Missing,
        Returned
    }

    public enum EquipmentConditionLevel
    {
        Good,
        Fair,
        Poor,
        Critical
    }

    public enum EquipmentTrackingType
    {
        QuantityBased,
        Individual
    }
}

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
        Missing
    }

    public enum EquipmentConditionLevel
    {
        Good,
        Fair,
        Poor,
        Broken
    }

    public enum EquipmentTrackingType
    {
        QuantityBased,
        Individual
    }
}

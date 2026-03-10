using System;

namespace CodeRoyale;

internal static class CommandHelper
{
    internal static object TranslateBuildingType(StructureType nextSitePriority)
    {
        switch (nextSitePriority)
        {
            case StructureType.BarracksArchers:
                return "BARRACKS-ARCHER";
            case StructureType.BarracksKnights:
                return "BARRACKS-KNIGHT";
            case StructureType.BarracksGiant:
                return "BARRACKS-GIANT";
            case StructureType.BarracksUnknown:
                return "BARRACKS-UNKNOWN";
            case StructureType.Empty:
                return "EMPTY";
            case StructureType.Tower:
                return "TOWER";
            case StructureType.Mine:
                return "MINE";
            default:
                throw new ArgumentOutOfRangeException(nameof(nextSitePriority), nextSitePriority, null);
        }
    }
}
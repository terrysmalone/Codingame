using System.Collections.Generic;

namespace _2023_1_SpringChallenge_Ants;

internal class ResourcePath
{
    internal int PathId { get; set; }
    internal int ParentPathId { get; set; } = -1;
    internal List<int> Path { get; set; } = new List<int>();
    public int BeaconStrength { get; }
    internal bool IsBasePath { get; set; } = false; // If the path is from a base to a resource
    internal CellType CellType { get; set; } = CellType.Empty; // Type of the resource at the end of the path

    internal ResourcePath(int pathId, int parentPathId, List<int> path, int beaconStrength, bool isBasePath, CellType cellType)
    {
        PathId = pathId;
        ParentPathId = parentPathId;
        Path = path;
        BeaconStrength = beaconStrength;
        IsBasePath = isBasePath;
        CellType = cellType;
    }
}




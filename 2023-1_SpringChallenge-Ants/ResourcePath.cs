using System.Collections.Generic;

namespace _2023_1_SpringChallenge_Ants;

internal class ResourcePath
{
    internal int Id { get; set; }
    internal int ParentId { get; set; }
    internal List<int> Path { get; set; } = new List<int>();
    public int BeaconStrength { get; }
    internal bool IsBasePath { get; set; } = false; // If the path is from a base to a resource
    internal bool IsEggPath { get; set; } = false; // If the path is to an egg
    internal bool IsCrystalPath { get; set; } = false; // If the path is to a crystal

    internal ResourcePath(int id, int parentId, List<int> path, int beaconStrength, bool isBasePath, bool isEggPath, bool isCrystalPath)
    {
        Id = id;
        ParentId = parentId;
        Path = path;
        BeaconStrength = beaconStrength;
        IsBasePath = isBasePath;
        IsEggPath = isEggPath;
        IsCrystalPath = isCrystalPath;
    }
}




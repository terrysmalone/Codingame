namespace _2023_1_SpringChallenge_Ants;

internal class Cell
{
    internal int Index { get; private set; }
    internal int[] NeighbourIds { get; private set; }
    internal CellType CellType { get; private set; }
    internal int EggCount { get; set; }
    internal int CrystalCount { get; set; }

    internal int playerAntsCount { get; set; } = 0;
    internal int opponentAntsCount { get; set; } = 0;

    internal Cell(int index, int[] neighbourIds, CellType cellType, int eggCount, int crystalCount)
    {
        Index = index;
        NeighbourIds = neighbourIds;
        CellType = cellType;
        EggCount = eggCount;
        CrystalCount = crystalCount;
    }
}

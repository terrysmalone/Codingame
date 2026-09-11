using System;

namespace BackTrackKing;

internal class Map {
    internal int Width { get; }
    internal int Height { get; }

    internal int[,] CellCosts;

    internal int[,] RegionIds;

    private int[,] _trackOwner;

    internal Map(int width, int height)
    {
        Width = width;
        Height = height;

        CellCosts = new int[width, height];

        RegionIds = new int[width, height];

        _trackOwner = new int[width, height];
    }

    internal void SetCell(int x, int y, CellType cellType, int region)
    {
        int cellCost = 0;

        switch (cellType)
        {
            case CellType.PLAINS:
                cellCost = 1;
                break;
            case CellType.RIVER:
                cellCost = 2;
                break;
            case CellType.MOUNTAIN:
                cellCost = 3;
                break;
            default:
                Logger.Error($"Unknown cell type: {cellType}");
                break;
        }

        CellCosts[x, y] = cellCost;

        RegionIds[x, y] = region;
    }

    internal void SetTrack(int x, int y, int tracksOwner)
    {
        _trackOwner[x, y] = tracksOwner;
    }

    internal bool isTrackFree(int x, int y)
    {
        return _trackOwner[x, y] == -1;
    }

    internal int GetTrackOwner(int x, int y)
    {
        return _trackOwner[x, y];
    }
}
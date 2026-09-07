using System;

namespace BackTrackKing;

internal class Map {
    internal int Width { get; }
    internal int Height { get; }

    internal CellType[,] CellTypes;
    internal int[,] Regions;


    internal Map(int width, int height)
    {
        Width = width;
        Height = height;

        CellTypes = new CellType[width, height];
        Regions = new int[width, height];
    }

    internal void SetCell(int x, int y, CellType cellType, int region)
    {
        CellTypes[x, y] = cellType;
        Regions[x, y] = region;
    }
}
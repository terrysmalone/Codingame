using System;

namespace BackTrackKing;

internal class Map {
    internal int Width { get; }
    internal int Height { get; }

    internal CellType[,] CellTypes;
    internal int[,] Regions;

    private int[,] _trackOwner;


    internal Map(int width, int height)
    {
        Width = width;
        Height = height;

        CellTypes = new CellType[width, height];
        Regions = new int[width, height];

        _trackOwner = new int[width, height];
    }

    internal void SetCell(int x, int y, CellType cellType, int region)
    {
        CellTypes[x, y] = cellType;
        Regions[x, y] = region;
    }

    internal void SetTrack(int x, int y, int tracksOwner)
    {
        _trackOwner[x, y] = tracksOwner;
    }

    internal bool isTrackFree(int x, int y)
    {
        return _trackOwner[x, y] == -1;
    }
}
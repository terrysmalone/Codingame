using System;
using System.Collections.Generic;
using System.Drawing;

namespace _2026_1_WinterChallenge_SnakeByte;

internal class Level
{
    private int width;
    private int height;

    internal bool[,] Platforms { get; private set; }

    private HashSet<Point> _allPlatformPositions = new HashSet<Point>();
    private HashSet<Point> _walkableLedges = new HashSet<Point>();

    internal HashSet<Point> PowerSources { get; private set; } = new HashSet<Point>();

    public Level(int width, int height, bool[,] platforms)
    {
        this.width = width;
        this.height = height;

        Platforms = platforms;

        CalculateAllPlatformPositions();
        CalculateWalkableLedges();
    }

    private void CalculateAllPlatformPositions()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (Platforms[y, x])
                {
                    _allPlatformPositions.Add(new Point(x, y));
                }
            }
        }
    }

    private void CalculateWalkableLedges()
    {
        // Only go to height minus 1 because we'll never want to check the ground for
        // climbing to
        for (int y = 1; y < height-1; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (Platforms[y, x])
                {
                    if(!Platforms[y-1, x])
                    {
                        _walkableLedges.Add(new Point(x, y-1));
                    }
                }
            }
        }
    }

    internal HashSet<Point> GetWalkableLedges()
    {
        return _walkableLedges;
    }

    internal bool IsPlatform(Point pointToCheck)
    {
        return Platforms[pointToCheck.Y, pointToCheck.X];
    }

    internal HashSet<Point> GetAllPlatformPositions()
    {
        return _allPlatformPositions;
    }
}

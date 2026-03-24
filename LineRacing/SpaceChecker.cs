using System;
using System.Collections.Generic;
using System.Drawing;

namespace LineRacing;

// Uses a flood fill to check how much space is in a given position
internal class SpaceChecker
{
    private bool[,] _grid;
    private MapChecker _mapChecker;

    internal SpaceChecker(bool[,] grid)
    {
        _grid = grid;
        _mapChecker = new MapChecker(grid);
    }

    internal int GetAvailableSpace(Point position)
    {
        var visited = new HashSet<Point>();
        var toCheck = new Queue<Point>();

        if(!_mapChecker.IsInBounds(position) || !_mapChecker.IsEmpty(position))
        {
            return 0;
        }

        toCheck.Enqueue(position);

        while (toCheck.Count > 0)
        {
            Point checkPoint = toCheck.Dequeue();

            foreach (Point adjacentPoint in _mapChecker.GetAdjacentPoints(checkPoint))
            {
                if (!visited.Contains(adjacentPoint) 
                    && _mapChecker.IsEmpty(adjacentPoint))
                {
                    toCheck.Enqueue(adjacentPoint);
                    visited.Add(adjacentPoint);
                }
            }
        }

        return visited.Count;
    }
}


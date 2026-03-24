using System.Collections.Generic;
using System.Drawing;

namespace LineRacing;

internal sealed class MapChecker
{
    private readonly bool[,] _grid;
    private readonly int _width;
    private readonly int _height;

    internal MapChecker(bool[,] grid)
    {
        _grid = grid;
        _height = _grid.GetLength(0);
        _width = _grid.GetLength(1);        
    }

    internal List<Point> GetAdjacentPoints(Point position)
    {
        List<Point> adjacentPoints = new List<Point>();

        if (position.X > 0)
        {
            adjacentPoints.Add(new Point(position.X - 1, position.Y));
        }

        if (position.X < _width - 1)
        {
            adjacentPoints.Add(new Point(position.X + 1, position.Y));
        }

        if (position.Y > 0)
        {
            adjacentPoints.Add(new Point(position.X, position.Y - 1));
        }

        if (position.Y < _height - 1)
        {
            adjacentPoints.Add(new Point(position.X, position.Y + 1));
        }

        return adjacentPoints;
    }

    internal bool IsInBounds(Point point)
    {
        return point.X >= 0 && point.X < _width && point.Y >= 0 && point.Y < _height;
    }

    internal bool IsEmpty(Point point)
    {
        return !_grid[point.Y, point.X];
    }
}



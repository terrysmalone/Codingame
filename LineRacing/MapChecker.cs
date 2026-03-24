using System;
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

    internal (List<Point>, List<Point>) GetHorizontalBlockingLinesFrom(Point point)
    {
        List<Point> leftBlockingLine = new List<Point>();
        List<Point> rightBlockingLine = new List<Point>();

        // Go left until blocked
        for (int xPos = point.X - 1; xPos >= 0; xPos--)
        {
            if (!_grid[point.Y, xPos])
            {
                leftBlockingLine.Add(new Point(xPos, point.Y));
            }
            else
            {
                break;
            }
        }

        // Go right until blocked
        for (int xPos = point.X + 1; xPos < _width; xPos++)
        {
            if (!_grid[point.Y, xPos])
            {
                rightBlockingLine.Add(new Point(xPos, point.Y));
            }
            else
            {
                break;
            }
        }

        return (leftBlockingLine, rightBlockingLine);
    }

    internal (List<Point>, List<Point>) GetVerticalBlockingLinesFrom(Point point)
    {
        List<Point> upBlockingLine = new List<Point>();
        List<Point> downBlockingLine = new List<Point>();

        // Go up until blocked
        for (int yPos = point.Y - 1; yPos >= 0; yPos--)
        {
            if (!_grid[yPos, point.X])
            {
                upBlockingLine.Add(new Point(point.X, yPos));
            }
            else
            {
                break;
            }
        }
        // Go down until blocked
        for (int yPos = point.Y + 1; yPos < _height; yPos++)
        {
            if (!_grid[yPos, point.X])
            {
                downBlockingLine.Add(new Point(point.X, yPos));
            }
            else
            {
                break;
            }
        }
        return (upBlockingLine, downBlockingLine);
    }

    internal List<Point> GetBlockingLine(Point myPosition, Point move)
    {
        // We assume the first move is valid because it shouldn't have been passed in otherwise
        List<Point> beam = new List<Point>() { move };

        Point direction = new Point(move.X - myPosition.X, move.Y - myPosition.Y);

        bool edgeFound = false;
        Point lastPoint = move;

        while(!edgeFound)
        {
            Point nextPoint = new Point(lastPoint.X + direction.X, lastPoint.Y + direction.Y);

            if (IsInBounds(nextPoint) && IsEmpty(nextPoint))
            {
                beam.Add(nextPoint);
                lastPoint = nextPoint;
            }
            else
            {
                edgeFound = true;
            }
        }

        return beam;
    }

    internal int GetValidMoves(Point move)
    {
        int validMoves = 0;
        foreach (var adjacentPoint in GetAdjacentPoints(move))
        {
            if (IsEmpty(adjacentPoint))
            {
                validMoves++;
            }
        }
        return validMoves;
    }
}
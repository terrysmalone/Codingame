using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;

namespace LineRacing;

internal sealed class Game
{
    private int _width;
    private int _height;

    private bool[,] _grid;

    private Point _myPosition;
    private Point _enemyposition;

    private List<Point> _myPath = new List<Point>();

    private SpaceChecker _spaceChecker;
    private MapChecker _mapChecker;

    internal Game(int width, int height)
    {
        _width = width;
        _height = height;

        _grid = new bool[height, width];

        _spaceChecker = new SpaceChecker(_grid);
        _mapChecker = new MapChecker(_grid);
    }

    internal void UpdateMyPosition(Point myPosition)
    {
        _myPosition = myPosition;
        _myPath.Add(myPosition);
        _grid[_myPosition.Y, _myPosition.X] = true;        
    }  

    internal void UpdateEnemyPosition(Point enemyPosition)
    {
        _enemyposition = enemyPosition;
        _grid[_enemyposition.Y, _enemyposition.X] = true;
    }

    internal string GetNextMove()
    {
        // Bare minimum first pass solution
        // Go in a straight line until we hit a wall. then choose the side with the most space.

        int straightLinePositionX = _myPosition.X - (_myPosition.X - _myPath[_myPath.Count - 1].X);
        int straightLinePositionY = _myPosition.Y - (_myPosition.Y - _myPath[_myPath.Count - 1].Y);
        Point straightLinePosition = new Point(straightLinePositionX, straightLinePositionY);

        if(_mapChecker.IsInBounds(straightLinePosition) && _mapChecker.IsEmpty(straightLinePosition))
        {
            return GetDirection(_myPosition, straightLinePosition);
        }

        Dictionary<Point, int> spaceByAdjacentPoint = new Dictionary<Point, int>();

        List<Point> availableSpaces = _mapChecker.GetAdjacentPoints(_myPosition);

        foreach (Point availableSpace in availableSpaces)
        {
            if (_mapChecker.IsEmpty(availableSpace))
            {
                spaceByAdjacentPoint[availableSpace] = _spaceChecker.GetAvailableSpace(availableSpace);

                Console.Error.WriteLine(GetDirection(_myPosition, availableSpace) + " has " + spaceByAdjacentPoint[availableSpace] + " space");
            }
        }

        Point bestAdjacentPoint = spaceByAdjacentPoint.OrderByDescending(kvp => kvp.Value).First().Key;
        string bestDirection = GetDirection(_myPosition, bestAdjacentPoint);
        
        return bestDirection;
    }

    private string GetDirection(Point myPosition, Point point)
    {
        if (point.X < myPosition.X)
        {
            return "LEFT";
        }
        else if (point.X > myPosition.X)
        {
            return "RIGHT";
        }
        else if (point.Y < myPosition.Y)
        {
            return "UP";
        }
        else if (point.Y > myPosition.Y)
        {
            return "DOWN";
        }
        return string.Empty;
    }
}

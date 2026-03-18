using System;
using System.Collections.Generic;
using System.Drawing;

namespace _2026_1_WinterChallenge_SnakeByte;

internal sealed class PositionChecker
{
    private Game _game;
    private readonly Level _level;

    internal PositionChecker(Game game, Level level)
    {
        _game = game;
        _level = level;
    }

    internal bool IsOutOfMapBounds(Point pointToCheck)
    {
        if (pointToCheck.X < 0 || pointToCheck.X >= _game.Width || pointToCheck.Y < 0 || pointToCheck.Y >= _game.Height)
        {
            return true;
        }
        return false;
    }

    internal bool IsBlocking(Point newHeadPosition, SnakeBot snakeBot)
    {
        // Flood fill algorithm to check if the new position would block the snake in
        var visited = new HashSet<Point>();
        var queue = new Queue<Point>();

        queue.Enqueue(newHeadPosition);
        visited.Add(newHeadPosition);

        while (queue.Count > 0)
        {
            Point checkPoint = queue.Dequeue();

            var adjacentPoints = new List<Point>()
            {
                new Point(checkPoint.X + 1, checkPoint.Y),
                new Point(checkPoint.X - 1, checkPoint.Y),
                new Point(checkPoint.X, checkPoint.Y + 1),
                new Point(checkPoint.X, checkPoint.Y - 1)
            };

            foreach (var adjacentPoint in adjacentPoints)
            {
                if (adjacentPoint.X >= -1
                    && adjacentPoint.X <= _game.Width
                    && adjacentPoint.Y >= -1
                    && adjacentPoint.Y < _game.Height
                    && !IsPlatform(adjacentPoint)
                    && !IsPointInAnySnake(adjacentPoint, countTails: true, excludeSnakeId: snakeBot.Id)
                    && !IsPointInGivenSnake(snakeBot.Body, adjacentPoint, countTails: false)
                    && !visited.Contains(adjacentPoint))
                {
                    queue.Enqueue(adjacentPoint);
                    visited.Add(adjacentPoint);
                }
            }

            if (visited.Count > snakeBot.Body.Count)
            {
                return false;
            }
        }

        if (visited.Count < snakeBot.Body.Count)
        {
            return true;
        }

        return false;
    }

    internal int FloodFillCount(Point newHeadPosition, int excludeSnakeId, List<Point> includeBody, int cutOff)
    {
        var visited = new HashSet<Point>();
        var queue = new Queue<Point>();

        queue.Enqueue(newHeadPosition);
        visited.Add(newHeadPosition);

        while (queue.Count > 0 && visited.Count < cutOff)
        {
            Point checkPoint = queue.Dequeue();

            var adjacentPoints = new List<Point>()
            {
                new Point(checkPoint.X + 1, checkPoint.Y),
                new Point(checkPoint.X - 1, checkPoint.Y),
                new Point(checkPoint.X, checkPoint.Y + 1),
                new Point(checkPoint.X, checkPoint.Y - 1)
            };

            foreach (var adjacentPoint in adjacentPoints)
            {
                if (adjacentPoint.X >= -1
                    && adjacentPoint.X <= _game.Width
                    && adjacentPoint.Y >= -1
                    && adjacentPoint.Y < _game.Height
                    && !IsPlatform(adjacentPoint)
                    && !IsPointInAnySnake(adjacentPoint, countTails: true, excludeSnakeId: excludeSnakeId)
                    && !IsPointInGivenSnake(includeBody, adjacentPoint, countTails: true)
                    && !visited.Contains(adjacentPoint))
                {
                    queue.Enqueue(adjacentPoint);
                    visited.Add(adjacentPoint);
                }
            }
        }

        if(queue.Count == 0)
        {
            return visited.Count - 1;
        }
        else
        {
            return cutOff;
        }
    }

    internal bool IsPlatform(Point pointToCheck)
    {
        if (IsOutOfMapBounds(pointToCheck))
        {
            return false;
        }

        if (_level.IsPlatform(pointToCheck))
        {
            return true;
        }

        return false;
    }

    internal bool IsPointInAnySnake(Point pointToCheck, bool countTails, int excludeSnakeId=-1)
    {
        foreach (var snakeBot in _game.MySnakeBots)
        {
            if (excludeSnakeId >= 0 && snakeBot.Id == excludeSnakeId)
            {
                continue;
            }

            if (IsPointInGivenSnake(snakeBot.Body, pointToCheck, countTails))
            {
                return true;
            }
        }

        foreach (var snakeBot in _game.OpponentSnakeBots)
        {
            if (snakeBot.Body.Contains(pointToCheck))
            {
                if (IsPointInGivenSnake(snakeBot.Body, pointToCheck, countTails))
                {
                    return true;
                }
            }
        }

        return false;
    }

    internal bool IsPointInGivenSnake(List<Point> snakeBody, Point pointToCheck, bool countTails)
    {
        if (snakeBody.Contains(pointToCheck))
        {
            if (!countTails)
            {
                if (snakeBody[snakeBody.Count - 1] == pointToCheck)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return true;
            }
        }

        return false;
    }

    internal bool IsStuckMove(Point newHeadPosition, SnakeBot snakeBot)
    {
        if (snakeBot.IsStuck())
        {
            if (newHeadPosition == snakeBot.GetLastMove())
            {
                return true;
            }
        }

        return false;
    }

    internal int GetNearestPlatformDistance(Point pointToCheck, int excludeSnakeId)
    {
        int nearestPlatformDistance = int.MaxValue;

        for (int y=0; y < _game.Height; y++)
        {
            for (int x=0; x < _game.Width; x++)
            {
                if (_level.IsPlatform(new Point(x, y)))
                {
                    int distance = GetNearestPowerSourceDistance(pointToCheck, new Point(x, y));
                    
                    if (distance < nearestPlatformDistance)
                    {
                        nearestPlatformDistance = distance;
                    }
                }
            }
        }

        // Now check snakes
        foreach (var snakeBot in _game.MySnakeBots)
        {
            if (excludeSnakeId == snakeBot.Id)
            {
                continue;
            }

            foreach (var bodyPart in snakeBot.Body)
            {               
                int distance = GetNearestPowerSourceDistance(pointToCheck, bodyPart);
                    
                if (distance < nearestPlatformDistance)
                {
                    nearestPlatformDistance = distance;
                }
            }
        }

        foreach (var snakeBot in _game.OpponentSnakeBots)
        {
            foreach (var bodyPart in snakeBot.Body)
            {
                int distance = GetNearestPowerSourceDistance(pointToCheck, bodyPart);
                        
                if (distance < nearestPlatformDistance)
                {
                    nearestPlatformDistance = distance;
                }
            }
        }

        return nearestPlatformDistance;
    }

    private int GetNearestPowerSourceDistance(Point powerSourcePoint, Point checkPoint, bool excludeGravity = true)
    {
        int distance = int.MaxValue;
        // If the power up is lower than the platform we don't need to count vertical distance because gravity
        // can do some of the work
        if (excludeGravity && powerSourcePoint.Y >= checkPoint.Y)
        {
            distance = Math.Abs(powerSourcePoint.X - checkPoint.X);
        }
        else
        {
            distance = Math.Abs(powerSourcePoint.X - checkPoint.X) + Math.Abs(powerSourcePoint.Y - checkPoint.Y);
        }

        return distance;
    }

    internal Dictionary<Point, int> GetClosestPowerSourceToOpponentSnakeMap()
    {
        Dictionary<Point, int> closestPowerSourceToOpponentSnakeMap = new Dictionary<Point, int>();

        foreach (SnakeBot snakeBot in _game.OpponentSnakeBots)
        {
            int closestPowerSourceDistance = int.MaxValue;
            Point closestPowerSource = new Point(-1, -1);

            foreach (var powerSource in _game.GetPowerSources())
            {
                int distance = GetNearestPowerSourceDistance(powerSource, snakeBot.Body[0], excludeGravity: false);
                if (distance < closestPowerSourceDistance)
                {
                    closestPowerSourceDistance = distance;
                    closestPowerSource = powerSource;
                }
            }
            
            if (closestPowerSource != new Point(-1, -1))
            {
                closestPowerSourceToOpponentSnakeMap[closestPowerSource] = snakeBot.Id;
            }
        }

        return closestPowerSourceToOpponentSnakeMap;
    }
}

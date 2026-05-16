using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;

namespace SpringChallenge2026;

internal class PositionUtil
{
    private readonly Game _game;
    private readonly PathFinder _pathFinder;

    private List<Point> _bestGrowSpots = new List<Point>();

    public PositionUtil(Game game, PathFinder pathFinder)
    {
        _game = game;
        _pathFinder = pathFinder;
    }

    internal List<Point> GetShortestPath(Point startPos, Point endPos)
    {
        return _pathFinder.GetShortestPath(startPos, endPos);
    }

    internal int GetClosestTreeToShack(ResourceType fruitType)
    {
        List<Tree> treesOfCorrectType = _game.GetTrees(fruitType);

        int closest = int.MaxValue;

        foreach (Tree tree in treesOfCorrectType)
        {
            int dist = _pathFinder.GetShortestPath(_game.GetPlayerShackPosition(), tree.Position).Count;
            if (dist < closest)
            {
                closest = dist;
            }
        }

        return closest;
    }

    internal void InitialiseBestGrowSpots()
    {
        const int MIN_DIST = 3;
        Point shackPos = _game.GetPlayerShackPosition();

        Logger.Message($"Initialising best grow spots around shack at {shackPos.X}, {shackPos.Y}");

        List<Point> validSpots = new List<Point>();

        for (int x = shackPos.X - 3; x <= shackPos.X + 3; x++)
        {
            for (int y = shackPos.Y - 3; y <= shackPos.Y + 3; y++)
            {
                if (x == shackPos.X && y == shackPos.Y)
                {
                    continue; // Skip the shack position itself
                }

                Point checkPoint = new Point(x, y);
                if (_game.IsInBounds(checkPoint) && _game.IsGrowable(checkPoint))
                {
                    validSpots.Add(checkPoint);
                }
            }
        }   

        // order valid spots by pathfinder distance to shack
        List<(int,Point)> distancesMap = new List<(int, Point)>();

        foreach (Point spot in validSpots)
        {
            int dist = _pathFinder.GetShortestPath(shackPos, spot).Count;

            if (dist <= MIN_DIST)
            distancesMap.Add((dist, spot));
        }

        // Order so that all spots adjacent to water are first (ordered by closest to shack), then order the rest by closerst to sack
        // This is because we want to prioritise growing trees adjacent to water, but if there are none available, we want to grow trees as close to the shack as possible
        _bestGrowSpots = distancesMap.OrderByDescending(d => IsAdjacentToWater(d.Item2)).ThenBy(d => d.Item1).Select(d => d.Item2).ToList();

        foreach (Point spot in _bestGrowSpots)
        {
            Console.Error.WriteLine($"Best grow spot: {spot.X}, {spot.Y}");
        }
    }

    private bool IsAdjacentToWater(Point point)
    {
        List<Point> adjacentPoints = new List<Point>
        {
            new Point(point.X - 1, point.Y),
            new Point(point.X + 1, point.Y),
            new Point(point.X, point.Y - 1),
            new Point(point.X, point.Y + 1)
        };

        return adjacentPoints.Any(p => _game.IsInBounds(p) && _game.IsWater(p));
    }

    internal (Troll? closestTroll, List<Point> shortestPath) GetClosestTrollToTargets(List<Troll> candidateTrolls, List<Point> candidatePoints, int cutoff = int.MaxValue)
    {
        int closestDistance = int.MaxValue;
        Troll? closestTroll = null;
        List<Point> pathToTarget = new List<Point>();

        foreach (Point tree in candidatePoints)
        {
            Logger.Message($"Getting closest troll to target at {tree.X}, {tree.Y}");
            (Troll? troll, List<Point> path) = GetClosestTrollToTarget(candidateTrolls, tree, Math.Min(closestDistance, cutoff));

            Logger.Message($"Closest troll to target at {tree.X}, {tree.Y} is troll {troll?.Id} with path length {path.Count}");
            if (path.Count < closestDistance)
            {
                closestDistance = path.Count;
                closestTroll = troll;
                pathToTarget = path;
            }
        }
        
        return (closestTroll, pathToTarget);
    }

    internal (Troll?, List<Point>)  GetClosestTrollToTarget(List<Troll> trolls, Point target, int cutoff = int.MaxValue)
    {
        int closestDistance = int.MaxValue;
        Troll? closestTroll = null;
        List<Point> pathToTarget = new List<Point>();

        // Order trolls by closest manhattan distance to target first
        trolls = trolls.OrderBy(t => CalculateManhattanDistance(t.Position, target)).ToList();

        foreach (Troll troll in trolls)
        {
            if (troll.Position == target)
            {
                return (troll, new List<Point> { troll.Position });
            }

            if (closestTroll != null && (CalculateManhattanDistance(troll.Position, target) >= closestDistance || CalculateManhattanDistance(troll.Position, target) >= cutoff))
            {
                Logger.Message($"Cut OFF: Troll {closestTroll.Id}");
                return (closestTroll, pathToTarget);
            }

            Logger.Message($"Calculating path from troll {troll.Id} at {troll.Position.X}, {troll.Position.Y} to target at {target.X}, {target.Y}");
            List<Point> path = _pathFinder.GetShortestPath(troll.Position, target);
            Logger.Message($"Path length: {path.Count}");

            if (path.Count < closestDistance)
            {
                Logger.Message($"New closest troll {troll.Id} at {troll.Position.X}, {troll.Position.Y} with path length {path.Count}");
                closestDistance = path.Count;
                closestTroll = troll;
                pathToTarget = path;
            }
        }

        return (closestTroll, pathToTarget);
    }

    private int CalculateManhattanDistance(Point position1, Point position2)
    {
        return Math.Abs(position1.X - position2.X) + Math.Abs(position1.Y - position2.Y);
    }

    internal bool IsTreeAtPosition(Point position, ResourceType fruitType)
    {
        return _game.GetTrees(fruitType).Any(t => t.Position == position);
    }

    internal Point GetBestGrowSpot()
    {

        // TODO: Order by closest to current position (and next to water)
        foreach (Point growSpot in _bestGrowSpots)
        {
            if (!_game.HasTree(growSpot))
            {
                return growSpot;
            }
        }
            
        Logger.Error("No valid grow spots found!");
        return new Point(-1, -1);
    }

    internal bool IsAdjacentToShack(Point position)
    {
        Point shackPos = _game.GetPlayerShackPosition();
        List<Point> adjacentPoints = new List<Point>
        {
            new Point(shackPos.X - 1, shackPos.Y),
            new Point(shackPos.X + 1, shackPos.Y),
            new Point(shackPos.X, shackPos.Y - 1),
            new Point(shackPos.X, shackPos.Y + 1)
        };
        return adjacentPoints.Any(p => p == position);
    }

    internal bool IsAdjacentTo(Point position, Point target)
    {
        List<Point> adjacentPoints = new List<Point>
        {
            new Point(target.X - 1, target.Y),
            new Point(target.X + 1, target.Y),
            new Point(target.X, target.Y - 1),
            new Point(target.X, target.Y + 1)
        };
        return adjacentPoints.Any(p => p == position);
    }
}

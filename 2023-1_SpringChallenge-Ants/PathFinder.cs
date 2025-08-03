using System;
using System.Collections;
using System.Collections.Generic;

namespace _2023_1_SpringChallenge_Ants;

internal class PathFinder
{
    private int idCounter = 0;
    private readonly Dictionary<int, Cell> _cells;    

    public PathFinder(Dictionary<int, Cell> cells)
    {
        _cells = cells;
    }

    internal List<ResourcePath> GetShortestPaths(List<StartReference> startReferences, List<SimpleCell> targetCells)
    {
        return GetShortestPaths(startReferences, targetCells, new List<int>());
    }

    internal List<ResourcePath> GetShortestPaths(List<StartReference> startReferences, List<SimpleCell> targetCells, List<int> targetCellsToExclude)
    {
        var paths = new List<ResourcePath>();

        foreach (StartReference startReference in startReferences)
        {
            foreach (var targetCell in targetCells)
            {
                if (startReference.CellId == targetCell.Id)
                {
                    continue; // Skip if start and target are the same
                }

                if (targetCellsToExclude.Contains(targetCell.Id))
                {
                    continue; // Skip excluded target cells
                }

                List<int> path = FindShortestPath(startReference.CellId, targetCell.Id, int.MaxValue);
                if (path.Count > 0)
                {
                    bool isBase = startReference.PathId == -1;
                    paths.Add(new ResourcePath(idCounter, startReference.PathId, path, 1, isBase, targetCell.CellType));
                    idCounter++;
                }
            }
        }

        return paths;
    }

    internal List<int> FindShortestPath(int start, List<int> targets)
    {
        var shortestPath = new List<int>();
        var shortestLength = int.MaxValue;

        foreach (var target in targets)
        {
            var path = FindShortestPath(start, target, shortestLength);
            if (path.Count > 0 && path.Count < shortestLength)
            {
                shortestPath = path;
                shortestLength = path.Count;
            }
        }

        return shortestPath;
    }

    internal List<int> FindShortestPath(int start, int target, int cutoff)
    {
        var path = new List<int>();
        var visited = new HashSet<int>();
        var parent = new Dictionary<int, int>();

        var queue = new Queue<int>();
        queue.Enqueue(start);
        visited.Add(start);
        
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (current == target)
            {
                while (current != start)
                {
                    path.Add(current);

                    if (path.Count > cutoff)
                    {
                        Console.Error.WriteLine($"Path from {start} to {target} exceeds cutoff of {cutoff}");
                        return new List<int>();
                    }

                    current = parent[current];
                }

                path.Add(start);
                path.Reverse();

                return path;
            }
            if (_cells.ContainsKey(current))
            {
                foreach (var neighbourId in _cells[current].NeighbourIds)
                {
                    if (!visited.Contains(neighbourId))
                    {
                        visited.Add(neighbourId);
                        queue.Enqueue(neighbourId);
                        parent[neighbourId] = current;
                    }
                }
            }
        }

        Console.Error.WriteLine($"ERROR: No path found from {start} to {target}");
        return new List<int>();
    }

    internal List<int> FindShortestTargetedPathToBase(int start, List<int> playerBases, Dictionary<int, int> targetedCells)
    {
        var shortestPath = new List<int>();
        var shortestLength = int.MaxValue;

        foreach (var playerBase in playerBases)
        {
            var path = FindShortestTargetedPath(start, playerBase, targetedCells, shortestLength);
            if (path.Count > 0 && path.Count < shortestLength)
            {
                shortestPath = path;
                shortestLength = path.Count;
            }
        }

        if (shortestPath.Count == 0)
        {
            Console.Error.WriteLine($"ERROR: No path found from {start} to any of the targets: {string.Join(", ", playerBases)}");
        }

        return shortestPath;
    }

    // Finds the shortest path in targetedCells
    private List<int> FindShortestTargetedPath(int start, int target, Dictionary<int, int> targetedCells, int cutoff)
    {
        var path = new List<int>();
        var visited = new HashSet<int>();
        var parent = new Dictionary<int, int>();

        var queue = new Queue<int>();
        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (current == target)
            {
                while (current != start)
                {
                    path.Add(current);

                    if (path.Count > cutoff)
                    {
                        Console.Error.WriteLine($"Path from {start} to {target} exceeds cutoff of {cutoff}");
                        return new List<int>();
                    }

                    current = parent[current];
                }

                path.Add(start);
                path.Reverse();

                return path;
            }
            if (targetedCells.ContainsKey(current))
            {
                foreach (var neighbourId in _cells[current].NeighbourIds)
                {
                    if (!visited.Contains(neighbourId))
                    {
                        visited.Add(neighbourId);
                        queue.Enqueue(neighbourId);
                        parent[neighbourId] = current;
                    }
                }
            }
        }

        Console.Error.WriteLine($"ERROR: No path found from {start} to {target}");
        return new List<int>();
    }

    internal List<int> FindShortestOpponentPathToBase(int start, List<int> targets)
    {
        var shortestPath = new List<int>();
        var shortestLength = int.MaxValue;

        foreach (var target in targets)
        {
            var path = FindShortestOpponentPath(start, target, shortestLength);
            if (path.Count > 0 && path.Count < shortestLength)
            {
                shortestPath = path;
                shortestLength = path.Count;
            }
        }

        if (shortestPath.Count == 0)
        {
            Console.Error.WriteLine($"No path found from {start} to any of the targets: {string.Join(", ", targets)}");
        }

        return shortestPath;
    }

    private List<int> FindShortestOpponentPath(int start, int target, int cutoff)
    {
        var path = new List<int>();
        var visited = new HashSet<int>();
        var parent = new Dictionary<int, int>();

        var queue = new Queue<int>();
        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (current == target)
            {
                while (current != start)
                {
                    path.Add(current);

                    if (path.Count > cutoff)
                    {
                        Console.Error.WriteLine($"Path from {start} to {target} exceeds cutoff of {cutoff}");
                        return new List<int>();
                    }

                    current = parent[current];
                }

                path.Add(start);
                path.Reverse();

                return path;
            }
            if (_cells.ContainsKey(current) && _cells[current].opponentAntsCount > 0)
            {
                foreach (var neighbourId in _cells[current].NeighbourIds)
                {
                    if (!visited.Contains(neighbourId))
                    {
                        visited.Add(neighbourId);
                        queue.Enqueue(neighbourId);
                        parent[neighbourId] = current;
                    }
                }
            }
        }

        return new List<int>();
    }
}




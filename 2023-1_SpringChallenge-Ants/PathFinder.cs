using System;
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

                List<int> path = FindShortestPath(startReference.CellId, targetCell.Id);
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
            var path = FindShortestPath(start, target);
            if (path.Count > 0 && path.Count < shortestLength)
            {
                shortestPath = path;
                shortestLength = path.Count;
            }
        }

        if (shortestPath.Count == 0)
        {
            Console.Error.WriteLine($"ERROR: No path found from {start} to any of the targets: {string.Join(", ", targets)}");
        }

        return shortestPath;
    }

    internal List<int> FindShortestPath(int start, int target)
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
}




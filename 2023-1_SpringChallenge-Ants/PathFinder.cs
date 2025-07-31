using System;
using System.Collections.Generic;

namespace _2023_1_SpringChallenge_Ants;

internal class PathFinder
{
    private readonly Dictionary<int, Cell> _cells;    

    public PathFinder(Dictionary<int, Cell> cells)
    {
        _cells = cells;
    }

    internal List<List<int>> GetShortestPaths(List<int> startCells, Dictionary<int, int> targetCells)
    {
        return GetShortestPaths(startCells, targetCells, new List<int>());
    }

    internal List<List<int>> GetShortestPaths(List<int> startCells, Dictionary<int, int> targetCells, List<int> targetCellsToExclude)
    {
        var paths = new List<List<int>>();

        foreach (int startCell in startCells)
        {
            foreach (var targetCell in targetCells)
            {
                if (startCell == targetCell.Key)
                {
                    continue; // Skip if start and target are the same
                }

                if (targetCellsToExclude.Contains(targetCell.Key))
                {
                    continue; // Skip excluded target cells
                }

                List<int> path = FindShortestPath(startCell, targetCell.Key);
                if (path.Count > 0)
                {
                    paths.Add(path);
                }
            }
        }

        return paths;
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




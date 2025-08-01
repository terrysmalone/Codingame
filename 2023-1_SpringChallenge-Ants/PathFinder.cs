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

    internal List<ResourcePath> GetShortestPaths(List<StartReference> startCells, Dictionary<int, int> targetCells, CellType targetType)
    {
        return GetShortestPaths(startCells, targetCells, new List<int>(), targetType);
    }

    internal List<ResourcePath> GetShortestPaths(List<StartReference> startReferences, Dictionary<int, int> targetCells, List<int> targetCellsToExclude, CellType targetType)
    {
        var paths = new List<ResourcePath>();

        foreach (StartReference startReference in startReferences)
        {
            foreach (var targetCell in targetCells)
            {
                if (startReference.CellId == targetCell.Key)
                {
                    continue; // Skip if start and target are the same
                }

                if (targetCellsToExclude.Contains(targetCell.Key))
                {
                    continue; // Skip excluded target cells
                }

                List<int> path = FindShortestPath(startReference.CellId, targetCell.Key);
                if (path.Count > 0)
                {
                    bool isBase = startReference.ParentId == -1;
                    bool isEggType = targetType == CellType.Egg;
                    bool isCrystalType = targetType == CellType.Crystal;


                    paths.Add(new ResourcePath(idCounter, startReference.ParentId, path, 1, isBase, isEggType, isCrystalType));
                    idCounter++;
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




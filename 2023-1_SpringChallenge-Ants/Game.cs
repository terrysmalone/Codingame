using System;
using System.Collections.Generic;

namespace _2023_1_SpringChallenge_Ants;

internal class Game
{
    private readonly int _numberOfCells;

    private List<int> _playerBases = new List<int>();
    private List<int> _opponentBases = new List<int>();

    private Dictionary<int, Cell> _cells = new Dictionary<int, Cell>();

    // Keep track of cell counts at the start of every loop for fast search
    private Dictionary<int, int> _eggCells = new Dictionary<int, int>();
    private Dictionary<int, int> _crystalCells = new Dictionary<int, int>();

    private PathFinder _pathFinder;



    public Game(int numberOfCells)
    {
        this._numberOfCells = numberOfCells;
        _pathFinder = new PathFinder(_cells);
    }

    internal void AddCell(int index, Cell cell)
    {
        _cells.Add(index, cell);
    }

    internal void AddPlayerBase(int myBaseIndex)
    {
        _playerBases.Add(myBaseIndex);
    }

    internal void AddOpponentBase(int oppBaseIndex)
    {
        _opponentBases.Add(oppBaseIndex);
    }

    internal void UpdateCell(int i, int resources, int myAnts, int oppAnts)
    {
        if (_cells.ContainsKey(i))
        {
            var cell = _cells[i];

            if (cell.CellType == CellType.Egg)
            {
                cell.EggCount = resources;
                if (resources > 0)
                {
                    _eggCells.Add(i, resources);
                }
            }
            else if (cell.CellType == CellType.Crystal)
            {
                cell.CrystalCount = resources;
                if (resources > 0)
                {
                    _crystalCells.Add(i, resources);
                }
            }

            cell.playerAntsCount = myAnts;
            cell.opponentAntsCount = oppAnts;
        }
        else
        {
            Console.Error.WriteLine($"ERROR: Cell {i} not found");
        }
    }

    internal void ResetCounts()
    {
        _eggCells.Clear();
        _crystalCells.Clear();
    }

    internal List<string> GetActions()
    {
        var actions = new List<string>();
        Console.Error.WriteLine($"EggCells: {_eggCells.Count}, CrystalCells: {_crystalCells.Count}");

        // First pass solution
        // For each base reserve half for collecting crystals and half ants for collecting eggs
        for (int i = 0; i < _playerBases.Count; i++)
        {
            List<List<int>> pathsToEggs = _pathFinder.GetShortestPaths(_playerBases[i], _eggCells);
            Display.Paths($"All egg paths from base {_playerBases[i]}", pathsToEggs);

            List<List<int>> pathsToCrystals = _pathFinder.GetShortestPaths(_playerBases[i], _crystalCells);
            Display.Paths($"All crystal paths from base {_playerBases[i]}", pathsToCrystals);

            List<int> shortestEggPath = GetShortestPath(pathsToEggs);
            Display.Path("Shortest egg path", shortestEggPath);

            List<int> shortestCrystalPath = GetShortestPath(pathsToCrystals);
            Display.Path("Shortest crystal path", shortestCrystalPath);

            foreach (var cell in shortestEggPath)
            {
                actions.Add($"BEACON {cell} 1");
            }

            foreach (var cell in shortestCrystalPath)
            {
                actions.Add($"BEACON {cell} 2");
            }
        }

        return actions;
    }
    
    private List<int> GetShortestPath(List<List<int>> paths)
    {
        if (paths.Count == 0)
        {
            return new List<int>();
        }
        // Find the shortest path
        List<int> shortestPath = paths[0];
        foreach (var path in paths)
        {
            if (path.Count < shortestPath.Count)
            {
                shortestPath = path;
            }
        }
        return shortestPath;
    }
}

internal class PathFinder
{
    private readonly Dictionary<int, Cell> _cells;

    public PathFinder(Dictionary<int, Cell> cells)
    {
        _cells = cells;
    }
    internal List<List<int>> GetShortestPaths(int start, Dictionary<int, int> targetCells)
    {
        var paths = new List<List<int>>();

        foreach (var targetCell in targetCells)
        {
            List<int> path = FindShortestPath(start, targetCell.Key);
            if (path.Count > 0)
            {
                paths.Add(path);
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


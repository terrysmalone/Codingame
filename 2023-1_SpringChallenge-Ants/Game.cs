using System;
using System.Collections.Generic;
using System.Linq;

namespace _2023_1_SpringChallenge_Ants;

internal class Game
{
    private readonly int _numberOfCells;

    private List<int> _playerBases = new List<int>();
    private List<int> _opponentBases = new List<int>();

    private Dictionary<int, Cell> _cells = new Dictionary<int, Cell>();

    private PathFinder _pathFinder;

    // Keep track of cell counts at the start of every loop for fast search
    private Dictionary<int, int> _eggCells = new Dictionary<int, int>();
    private Dictionary<int, int> _crystalCells = new Dictionary<int, int>();


    private int _totalEggCount = 0;
    private int _totalCrystalCount = 0;

    private int _playerAntCount = 0;
    private int _opponentAntCount = 0;

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
                    _totalEggCount += resources;
                }
            }
            else if (cell.CellType == CellType.Crystal)
            {
                cell.CrystalCount = resources;
                if (resources > 0)
                {
                    _crystalCells.Add(i, resources);
                    _totalCrystalCount += resources;
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

        _playerAntCount = 0;
        _opponentAntCount = 0;
        _totalEggCount = 0;
        _totalCrystalCount = 0;
    }

    internal List<string> GetActions()
    {
        var eggPathLimit = 5;
        var crystalPathLimit = 5;

        var actions = new List<string>();

        // For each base reserve half for collecting crystals and half ants for collecting eggs
        var currentEggSeekingAnts = _playerAntCount / 2;
        var currentCrystalSeekingAnts = _playerAntCount - currentEggSeekingAnts;

        var antsPerBase = _playerAntCount / _playerBases.Count;

        var targetedCells = new Dictionary<int, int>();

        foreach (int playerBase in _playerBases)
        {
            var startPoints = new List<int> { playerBase };

            var availableAnts = antsPerBase;

            var availableEggAnts = availableAnts / 2;            
            Dictionary<int, int> targetedEggCells = CalculateCellTargets(startPoints, _eggCells, availableEggAnts, eggPathLimit);

            var availableCrystalAnts = availableAnts - targetedEggCells.Count; // TODO: We need to count actual amounts here
            Dictionary<int, int> targetedCrystalCells = CalculateCellTargets(startPoints, _crystalCells, availableCrystalAnts, crystalPathLimit);

            AddToTargetedCells(targetedCells, targetedEggCells);
            AddToTargetedCells(targetedCells, targetedCrystalCells);
        }

        actions = GetBeaconActions(targetedCells);

        return actions;
    }

    private Dictionary<int, int> CalculateCellTargets(List<int> startPoints, Dictionary<int, int> resourceCells, int availableResourceAnts, int resourcePathLimit)
    {
        var resourcePathCount = 0;

        var targetedResourceCells = new Dictionary<int, int>();

        // Get resource paths
        var targetedResource = new List<int>();
        var targetedResourceCount = 0;
        while (availableResourceAnts > targetedResourceCount && resourcePathCount <= resourcePathLimit)
        {
            var targetAmount = 1;

            List<List<int>> pathsToResources = _pathFinder.GetShortestPaths(startPoints, resourceCells, targetedResource);

            List<int> shortestResourcePath = GetShortestPath(pathsToResources);

            if (shortestResourcePath.Count == 0)
            {
                break; // No path found, stop looking
            }

            startPoints.Add(shortestResourcePath[shortestResourcePath.Count - 1]);
            targetedResource.Add(shortestResourcePath[shortestResourcePath.Count - 1]);

            foreach (var cell in shortestResourcePath)
            {
                if (targetedResourceCells.ContainsKey(cell))
                {
                    // If the cell is already targeted check if we're now targeting it for more
                    if (targetedResourceCells[cell] < targetAmount)
                    {
                        var increaseAmount = targetAmount - targetedResourceCells[cell];
                        targetedResourceCells[cell] = targetAmount;
                        targetedResourceCount += increaseAmount;
                    }
                }
                else
                {
                    targetedResourceCells.Add(cell, targetAmount);
                    targetedResourceCount += targetAmount;
                }
            }

            resourcePathCount++;
        }

        return targetedResourceCells;
    }

    private static List<int> GetShortestPath(List<List<int>> paths)
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

    private static void AddToTargetedCells(Dictionary<int, int> targetedCells, Dictionary<int, int> targetedResourceCells)
    {
        foreach (var cell in targetedResourceCells)
        {
            if (targetedCells.ContainsKey(cell.Key))
            {
                // If the cell is already targeted check if we're now targeting it for more
                if (targetedCells[cell.Key] < cell.Value)
                {
                    targetedCells[cell.Key] = cell.Value;
                }
            }
            else
            {
                targetedCells.Add(cell.Key, cell.Value);
            }
        }
    }

    private static List<string> GetBeaconActions(Dictionary<int, int> targetedCells)
    {
        var actions = new List<string>();

        foreach (KeyValuePair<int, int> targetedCell in targetedCells)
        {
            actions.Add($"BEACON {targetedCell.Key} {targetedCell.Value}");
        }

        return actions;
    }    

    internal void IncreasePlayerAntCount(int count)
    {
        _playerAntCount += count;
    }

    internal void IncreaseOpponentAntCount(int count)
    {
        _opponentAntCount += count;
    }
}




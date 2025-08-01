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
        // TODO: Do counts of beacons for both eggs and crystals together.
        //       Currently we do them separate 

        var actions = new List<string>();

        // For each base reserve half for collecting crystals and half ants for collecting eggs
        var currentEggSeekingAnts = _playerAntCount / 2;
        var currentCrystalSeekingAnts = _playerAntCount - currentEggSeekingAnts;

        var antsPerBase = _playerAntCount / _playerBases.Count;

        var currentEggPaths = new List<List<int>>();
        var currentCrystalPaths = new List<List<int>>();

        var targetedCells = new Dictionary<int, int>();

        foreach (int playerBase in _playerBases)
        {
            var eggPathCount = 0;
            var crystalPathCount = 0;

            var startPoints = new List<int> { playerBase };

            var availableAnts = antsPerBase;

            var availableEggAnts = availableAnts / 2;

            var targetedEggCells = new Dictionary<int, int>();

            // Get egg paths
            var targetedEggs = new List<int>();
            var targetedEggCount = 0;
            while (availableEggAnts > targetedEggCount && eggPathCount <= eggPathLimit)
            {
                var targetAmount = 1;

                List<List<int>> pathsToEggs = _pathFinder.GetShortestPaths(startPoints, _eggCells, targetedEggs);

                List<int> shortestEggPath = GetShortestPath(pathsToEggs);

                if (shortestEggPath.Count == 0)
                {
                    break; // No path found, stop looking
                }

                currentEggPaths.Add(shortestEggPath);

                startPoints.Add(shortestEggPath[shortestEggPath.Count - 1]);
                targetedEggs.Add(shortestEggPath[shortestEggPath.Count - 1]);

                foreach (var cell in shortestEggPath)
                {
                    if (targetedEggCells.ContainsKey(cell))
                    {
                        // If the cell is already targeted check if we're now targeting it for more
                        if (targetedEggCells[cell] < targetAmount)
                        {
                            var increaseAmount = targetAmount - targetedEggCells[cell];
                            targetedEggCells[cell] = targetAmount;
                            targetedEggCount += increaseAmount;
                        }
                    }
                    else
                    {
                        targetedEggCells.Add(cell, targetAmount);
                        targetedEggCount += targetAmount;
                    }
                }

                eggPathCount++;
            }

            // Display.Paths($"Egg paths from base {playerBase}", currentEggPaths);

            // Get crystal paths
            var targetedCrystals = new List<int>();
            var availableCrystalAnts = availableAnts - targetedEggCells.Count;

            var targetedCrystalCells = new Dictionary<int, int>();
            var targetedCrystalCount = 0;
            while (availableCrystalAnts > targetedCrystalCount && crystalPathCount <= crystalPathLimit)
            {
                var targetAmount = 1;
                List<List<int>> pathsToCrystals = _pathFinder.GetShortestPaths(startPoints, _crystalCells, targetedCrystals);

                List<int> shortestCrystalPath = GetShortestPath(pathsToCrystals);

                if (shortestCrystalPath.Count == 0)
                {
                    break; // No path found, stop looking
                }

                currentCrystalPaths.Add(shortestCrystalPath);

                startPoints.Add(shortestCrystalPath[shortestCrystalPath.Count - 1]);
                targetedCrystals.Add(shortestCrystalPath[shortestCrystalPath.Count - 1]);

                foreach (var cell in shortestCrystalPath)
                {
                    if (targetedCrystalCells.ContainsKey(cell))
                    {
                        // If the cell is already targeted check if we're now targeting it for more
                        if (targetedCrystalCells[cell] < targetAmount)
                        {
                            var increaseAmount = targetAmount - targetedCrystalCells[cell];
                            targetedCrystalCells[cell] = targetAmount;
                            targetedCrystalCount += increaseAmount;
                        }
                    }
                    else
                    {
                        targetedCrystalCells.Add(cell, targetAmount);
                        targetedCrystalCount += targetAmount;
                    }
                }

                crystalPathCount++;
            }

            // Add targetedEggCells to targetedCells
            foreach (var cell in targetedEggCells)
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

            // Add targetedCrystalCells to targetedCells
            foreach (var cell in targetedCrystalCells)
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

            // Display.Paths($"Crystal paths from base {playerBase}", currentCrystalPaths);
        }

        actions = GetBeaconActions(targetedCells);

        return actions;
    }

    private List<string> GetBeaconActions(Dictionary<int, int> targetedCells)
    {
        var actions = new List<string>();

        foreach (KeyValuePair<int, int> targetedCell in targetedCells)
        {
            actions.Add($"BEACON {targetedCell.Key} {targetedCell.Value}");
        }

        return actions;
    }

    private List<string> PathsToActions(List<List<int>> currentEggPaths)
    {
        HashSet<int> beaconedCells = new HashSet<int>();

        var actions = new List<string>();
        foreach (var path in currentEggPaths)
        {
            if (path.Count == 0)
            {
                continue;
            }

            for (int i = 0; i < path.Count; i++)
            {
                int cell = path[i];
                if (!beaconedCells.Contains(cell))
                {
                    actions.Add($"BEACON {cell} 1");
                    beaconedCells.Add(cell);
                }           
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

    internal void IncreasePlayerAntCount(int count)
    {
        _playerAntCount += count;
    }

    internal void IncreaseOpponentAntCount(int count)
    {
        _opponentAntCount += count;
    }
}




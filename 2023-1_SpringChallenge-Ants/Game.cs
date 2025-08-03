using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
    private List<SimpleCell> _resourceCells = new List<SimpleCell>();

    private int _totalEggCount = 0;
    private int _totalCrystalCount = 0;

    private int _totalEggCells = 0;
    private int _totalCrystalCells = 0;

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
                    _resourceCells.Add(new SimpleCell(i, cell.CellType, resources));
                    _totalEggCount += resources;
                    _totalEggCells++;
                }
            }
            else if (cell.CellType == CellType.Crystal)
            {
                cell.CrystalCount = resources;
                if (resources > 0)
                {
                    _resourceCells.Add(new SimpleCell(i, cell.CellType, resources));
                    _totalCrystalCount += resources;
                    _totalCrystalCells++;
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
        _resourceCells.Clear();

        _playerAntCount = 0;
        _opponentAntCount = 0;
        _totalEggCount = 0;
        _totalCrystalCount = 0;
        _totalEggCells = 0;
        _totalCrystalCells = 0;
    }

    internal List<string> GetActions()
    {
        var totalResourceCells = (_totalEggCells + _totalCrystalCells) / _playerBases.Count;

        var pathLimit = 10;

        Console.Error.WriteLine($"_totalEggCells: {_totalEggCells}, _totalCrystalCells: {_totalCrystalCells}");
        if (_totalEggCells + _totalCrystalCells <= 15)
        {
            pathLimit = 5; // If there are not many resources, limit the paths
        }

        if (_totalEggCells + _totalCrystalCells <= 10)
        {
            pathLimit = 3; // If there are not many resources, limit the paths
        }

        var actions = new List<string>();

        var antsPerBase = _playerAntCount / _playerBases.Count;

        var targetedCells = new Dictionary<int, int>();
        var targetedResources = new List<int>();

        foreach (int playerBase in _playerBases)
        {
            Console.Error.WriteLine($"Processing base {playerBase}");
            // Display the targetedResources
            Console.Error.WriteLine($"Targeted Resources for base {playerBase}: {string.Join(", ", targetedResources)}");

            var startPoints = new List<StartReference> { new StartReference(playerBase, -1, -1) };
            List<ResourcePath> resourcePaths = CalculateBestResourcePaths(startPoints, _resourceCells, pathLimit, targetedResources);
            Display.ResourcePaths("Resource Paths", resourcePaths);

            // We want to minimise number of ants while maximising resources
            var availableAnts = antsPerBase;
            var eggCellCount = 0;
            var crystalCellCount = 0;
            
            Console.Error.WriteLine($"Available Ants:{availableAnts}");

            List<int> parentPaths = new List<int>();

            while (resourcePaths.Count > 0 && availableAnts >= resourcePaths.First().Path.Count)
            { 
                // Get closest base to resource
                var closestResourcePath = GetClosestBaseToResourcePath(resourcePaths, parentPaths);

                // Display.ResourcePaths("Closest Resource Path", new List<ResourcePath> { closestResourcePath });
                if (closestResourcePath == null)
                {
                    break; // No resource paths available
                }

                Display.ResourcePaths("Closest Resource Path", new List<ResourcePath> { closestResourcePath });

                targetedResources.Add(closestResourcePath.Path[closestResourcePath.Path.Count - 1]);

                (List<int> fullPath, int neededStrength) = CalculateFullPathAndNeededStrength(closestResourcePath, targetedCells);


                if (neededStrength == 1)
                { 
                    foreach (int cellId in closestResourcePath.Path)
                    {
                        if (!targetedCells.ContainsKey(cellId))
                        {
                            targetedCells.Add(cellId, 1);
                            availableAnts--;
                        }
                    }

                    if (closestResourcePath.CellType == CellType.Egg)
                    {
                        eggCellCount++;
                    }
                    else if (closestResourcePath.CellType == CellType.Crystal)
                    {
                        crystalCellCount++;
                    }

                    parentPaths.Add(closestResourcePath.PathId);
                }
                else
                {
                    // TODO: Do it for full path....


                    Console.Error.WriteLine($"We need to increase chain strength to {neededStrength} for chain to {fullPath[fullPath.Count-1]}");


                    // Calculate the cost to increate the whole thing to chainStrength + 1
                    int neededAnts = 0;
                    foreach (int cell in fullPath)
                    {
                        if (_cells.ContainsKey(cell))
                        {
                            if (targetedCells.ContainsKey(cell) && targetedCells[cell] < neededStrength)
                            {
                                neededAnts += neededStrength - targetedCells[cell];
                            }
                            else
                            {
                                neededAnts += neededStrength;
                            }
                        }
                    }

                    // If we have enough, do it, otherwise skip this path
                    if (neededAnts <= availableAnts)
                    {
                        Console.Error.WriteLine($"Increasing chain strength to {neededStrength} for chain to {fullPath[fullPath.Count - 1]} - needed:{neededAnts} available:{availableAnts}");
                        foreach (int cellId in fullPath)
                        {
                            if (!targetedCells.ContainsKey(cellId))
                            {
                                targetedCells.Add(cellId, neededStrength);
                                availableAnts -= (neededStrength);
                            }
                            else
                            {
                                // If the cell is already targeted, increase the strength
                                var currentStrength = targetedCells[cellId];
                                targetedCells[cellId] = neededStrength;

                                availableAnts -= neededStrength - currentStrength;
                            }
                        }

                        if (closestResourcePath.CellType == CellType.Egg)
                        {
                            eggCellCount++;
                        }
                        else if (closestResourcePath.CellType == CellType.Crystal)
                        {
                            crystalCellCount++;
                        }

                        parentPaths.Add(closestResourcePath.PathId);

                    }
                    else
                    {
                        Console.Error.WriteLine($"Not enough ants to increase chain strength to {neededStrength} for chain to {fullPath[fullPath.Count - 1]} - needed:{neededAnts} available:{availableAnts}");
                    }
                }

                // Console.Error.WriteLine($"Available Ants:{availableAnts}-eggCellCount:{eggCellCount}-crystalCellCount:{crystalCellCount}");
            }

            Console.Error.WriteLine($"Spare ants: {availableAnts}");

            // As a first pass just redistribute the remaining ants to the targeted cells
            var counter = 0;
            while (availableAnts > 0)
            {
                var index = targetedCells.ElementAt(counter).Key;
                targetedCells[index] = targetedCells[index] + 1;

                counter++;
                if (counter >= targetedCells.Count)
                {
                    counter = 0;
                }

                availableAnts--;
            }

            Console.Error.WriteLine($"Spare ants: {availableAnts}");
        }

        actions = GetBeaconActions(targetedCells);

        return actions;
    }

    // Calculates the shortest path to a base and what strength is needed to be stronger than the opponent 
    private (List<int>, int) CalculateFullPathAndNeededStrength(ResourcePath closestResourcePath, Dictionary<int, int> targetedCells)
    {
        var fullPath = new List<int>();

        // Copy closestResourcePath in reverse order
        for (int i = closestResourcePath.Path.Count-1; i >= 0; i--)
        {
            fullPath.Add(closestResourcePath.Path[i]);
        }

        if (!closestResourcePath.IsBasePath)
        {
            List<int> pathToBase = _pathFinder.FindShortestTargetedPathToBase(fullPath[fullPath.Count - 1], _playerBases, targetedCells);

            if (pathToBase.Count >= 1)
            {
                for (int i = 1; i < pathToBase.Count; i++)
                {
                    fullPath.Add(pathToBase[i]);
                }
            }
        }

        int strongestEnemyChain = 0;

        foreach (var cell in fullPath)
        {
            if (_cells.ContainsKey(cell))
            {
                if (_cells[cell].opponentAntsCount > 0)
                {
                    int chainStrength = CalculateEnemyChainStrengthFrom(cell);

                    if (chainStrength > strongestEnemyChain)
                    {
                        strongestEnemyChain = chainStrength;
                    }
                }
            }
        }

        return (fullPath, strongestEnemyChain + 1);
    }

    private int CalculateEnemyChainStrengthFrom(int cell)
    {
        List<int> chain = _pathFinder.FindShortestOpponentPathToBase(cell, _opponentBases);

        if (chain.Count == 0)
        {
            return 0;
        }

        var strength = GetChainStrength(chain, false);

        return strength;
    }

    private int GetChainStrength(List<int> path, bool forPlayer)
    {
        var strength = int.MaxValue;

        foreach (var cell in path)
        {
            if (_cells.ContainsKey(cell))
            {
                if (forPlayer)
                {
                    if (_cells[cell].playerAntsCount < strength)
                    {
                        strength = _cells[cell].playerAntsCount;
                    }
                }
                else
                {
                    if (_cells[cell].opponentAntsCount < strength)
                    {
                        strength = _cells[cell].opponentAntsCount;

                    }
                }
            }
        }

        return strength;
    }

    private ResourcePath GetClosestBaseToResourcePath(List<ResourcePath> resourcePaths, List<int> parentPathsToInclude)
    {
        var possiblePaths = resourcePaths.Where(rp => rp.IsBasePath || parentPathsToInclude.Contains(rp.ParentPathId)).ToList();

        if (possiblePaths.Count == 0)
        {
            return null; // No paths available
        }

        var closestPath = possiblePaths.First();

        resourcePaths.Remove(closestPath);
        return closestPath;
    }

    private List<ResourcePath> CalculateBestResourcePaths(List<StartReference> startPoints, 
                                                          List<SimpleCell> resourceCells,                                                    
                                                          int resourcePathLimit,
                                                          List<int> excludedResources)
    {
        var resourcePathCount = 0;

        var resourcePaths = new List<ResourcePath>();

        // Get resource paths
        var targetedResource = new List<int>();

        foreach (var excludedResource in excludedResources)
        {
            targetedResource.Add(excludedResource);
        }
        while (resourcePathCount < resourcePathLimit)
        {
            List<ResourcePath> pathsToResources = _pathFinder.GetShortestPaths(startPoints, resourceCells, targetedResource);

            ResourcePath shortestResourcePath = GetShortestPath(pathsToResources);

            if (shortestResourcePath == null || shortestResourcePath.Path.Count == 0)
            {
                break; // No path found, stop looking
            }

            int targetId = shortestResourcePath.Path[shortestResourcePath.Path.Count - 1];

            startPoints.Add(new StartReference(targetId, shortestResourcePath.PathId, shortestResourcePath.ParentPathId));
            targetedResource.Add(targetId);

            resourcePaths.Add(shortestResourcePath);


            resourcePathCount++;
        }

        resourcePaths.Sort((a, b) => a.Path.Count.CompareTo(b.Path.Count));

        return resourcePaths;
    }

    private static ResourcePath GetShortestPath(List<ResourcePath> paths)
    {
        if (paths.Count == 0)
        {
            return null;
        }

        // Find the shortest path
        ResourcePath shortestPath = paths[0];
        foreach (ResourcePath path in paths)
        {
            if (path.Path.Count < shortestPath.Path.Count)
            {
                shortestPath = path;
            }
        }
        return shortestPath;
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

internal struct SimpleCell
{
    public int Id { get; set; }
    public CellType CellType { get; set; }
   internal int Resources { get; set; }

    public SimpleCell(int id, CellType cellType, int resources)
    {
        Id = id;
        CellType = cellType;
        Resources = resources;
    }
}
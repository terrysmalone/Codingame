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
                }
            }
            else if (cell.CellType == CellType.Crystal)
            {
                cell.CrystalCount = resources;
                if (resources > 0)
                {
                    _resourceCells.Add(new SimpleCell(i, cell.CellType, resources));
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
        _resourceCells.Clear();

        _playerAntCount = 0;
        _opponentAntCount = 0;
        _totalEggCount = 0;
        _totalCrystalCount = 0;
    }

    internal List<string> GetActions()
    {
        var pathLimit = 10;

        var actions = new List<string>();

        // For each base reserve half for collecting crystals and half ants for collecting eggs
        var currentEggSeekingAnts = _playerAntCount / 2;
        var currentCrystalSeekingAnts = _playerAntCount - currentEggSeekingAnts;

        var antsPerBase = _playerAntCount / _playerBases.Count;

        var targetedCells = new Dictionary<int, int>();

        foreach (int playerBase in _playerBases)
        {
            var startPoints = new List<StartReference> { new StartReference(playerBase, -1, -1) };
            List<ResourcePath> resourcePaths = CalculateBestResourcePaths(startPoints, _resourceCells, pathLimit);
            // Display.ResourcePaths("Resource Paths", resourcePaths);

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
                    Console.Error.WriteLine($"No closest resource path found for base {playerBase}");
                    break; // No resource paths available
                }

                // Calculate the attack chain from a resource to the nearest enemy base
                // TODO: We don't need to get the shortest path here. We need the shortest path that the opponent has ants on. It won't
                // always be the actual shortest path. THe same goes for the player base. 
                List<int> shortestOpponentPathToBase = _pathFinder.FindShortestPath(closestResourcePath.Path[closestResourcePath.Path.Count - 1],
                                                                                    _opponentBases);

                Console.Error.WriteLine($"Shortest path to opponent base from {closestResourcePath.Path[closestResourcePath.Path.Count - 1]}: {string.Join("->", shortestOpponentPathToBase)}");
                int chainStrength = GetChainStrength(shortestOpponentPathToBase, forPlayer:false);

                Console.Error.WriteLine($"Chain strength to opponent base: {chainStrength} for path {closestResourcePath.Path[closestResourcePath.Path.Count - 1]}");

                if (chainStrength == 0)
                { 
                    Console.Error.WriteLine($"Creating chain to {closestResourcePath.Path[closestResourcePath.Path.Count - 1]}");
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
                    Console.Error.WriteLine($"We need to increase chain strength to {chainStrength+1} for chain to {closestResourcePath.Path[closestResourcePath.Path.Count-1]}");

                    // Get the entire chain for the player resource path
                    List<int> shortestPlayerPathToBase = _pathFinder.FindShortestPath(closestResourcePath.Path[closestResourcePath.Path.Count - 1],
                                                                                      _playerBases);

                    // Calculate the cost to increate the whole thing to chainStrength + 1
                    int neededAnts = 0;
                    foreach (int cell in shortestPlayerPathToBase)
                    {
                        if (_cells.ContainsKey(cell) && _cells[cell].playerAntsCount < chainStrength + 1)
                        {
                            neededAnts += (chainStrength + 1) - _cells[cell].playerAntsCount;
                        }
                    }

                    // If we have enough, do it, otherwise skip this path
                    if (neededAnts <= availableAnts)
                    {
                        Console.Error.WriteLine($"Increasing chain strength to {chainStrength + 1} for chain to {closestResourcePath.Path[closestResourcePath.Path.Count - 1]} - needed:{neededAnts} available:{availableAnts}");
                        foreach (int cellId in closestResourcePath.Path)
                        {
                            if (!targetedCells.ContainsKey(cellId))
                            {
                                targetedCells.Add(cellId, chainStrength + 1);
                                availableAnts -= (chainStrength + 1);
                            }
                            else
                            {
                                // If the cell is already targeted, increase the strength
                                var currentStrength = targetedCells[cellId];
                                targetedCells[cellId] = chainStrength + 1;

                                availableAnts -= (chainStrength + 1) - currentStrength;
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
                        Console.Error.WriteLine($"Not enough ants to increase chain strength to {chainStrength + 1} for chain to {closestResourcePath.Path[closestResourcePath.Path.Count - 1]} - needed:{neededAnts} available:{availableAnts}");
                    }
                }

                // Console.Error.WriteLine($"Available Ants:{availableAnts}-eggCellCount:{eggCellCount}-crystalCellCount:{crystalCellCount}");
            }

            Console.Error.WriteLine($"Spare ants: {availableAnts}");

            // AddToTargetedCells(targetedCells, eggResourcePaths);
            // AddToTargetedCells(targetedCells, crystalResourcePaths);
        }

        actions = GetBeaconActions(targetedCells);

        return actions;
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
        Console.Error.WriteLine($"Finding closest base to resource path - remaining paths: {resourcePaths.Count}");
        var possiblePaths = resourcePaths.Where(rp => rp.IsBasePath || parentPathsToInclude.Contains(rp.ParentPathId)).ToList();

        if (possiblePaths.Count == 0)
        {
            Console.Error.WriteLine("No possible paths found");
            return null; // No paths available
        }

        var closestPath = possiblePaths.First();

        resourcePaths.Remove(closestPath);
        return closestPath;
    }

    private List<ResourcePath> CalculateBestResourcePaths(List<StartReference> startPoints, 
                                                          List<SimpleCell> resourceCells,                                                    
                                                          int resourcePathLimit)
    {
        var resourcePathCount = 0;

        var resourcePaths = new List<ResourcePath>();

        // Get resource paths
        var targetedResource = new List<int>();
        while (resourcePathCount <= resourcePathLimit)
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
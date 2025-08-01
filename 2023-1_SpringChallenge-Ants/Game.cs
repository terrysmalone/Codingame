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

            resourcePaths.Sort((a, b) => a.Path.Count.CompareTo(b.Path.Count));
            Display.ResourcePaths("Resource Paths", resourcePaths);

            // We want to minimise number of ants while maximising resources
            var availableAnts = antsPerBase;
            var eggCellCount = 0;
            var crystalCellCount = 0;
            
            Console.Error.WriteLine($"Available Ants:{availableAnts}-eggCellCount:{eggCellCount}-crystalCellCount:{crystalCellCount}");

            // Get closest base to resource
            var closestResourcePath = GetClosestBaseToResourcePath(resourcePaths);

            Display.ResourcePaths("Closest Resource Path", new List<ResourcePath> { closestResourcePath });
            if (closestResourcePath == null)
            {
                Console.Error.WriteLine($"No closest resource path found for base {playerBase}");
                continue; // No resource paths available
            }

            // TODO: If enemy ants are on the target cell try to increase amount

            // Get the resource type and count
            if (closestResourcePath.CellType == CellType.Egg)
            {
                eggCellCount++;
            }
            else if (closestResourcePath.CellType == CellType.Crystal)
            {
                crystalCellCount++;
            }

            availableAnts -= closestResourcePath.Path.Count;

            Console.Error.WriteLine($"Available Ants:{availableAnts}-eggCellCount:{eggCellCount}-crystalCellCount:{crystalCellCount}");


            while (availableAnts >= resourcePaths.First().Path.Count)
            {
                //    Get closest base/resource to resource
                //    Increment that resource type
                //    Decrement availableAnts
            }

            // AddToTargetedCells(targetedCells, eggResourcePaths);
            // AddToTargetedCells(targetedCells, crystalResourcePaths);
        }

        actions = GetBeaconActions(targetedCells);

        return actions;
    }

    private ResourcePath GetClosestBaseToResourcePath(List<ResourcePath> resourcePaths)
    {
        var possiblePaths = resourcePaths.Where(rp => rp.IsBasePath).ToList();

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
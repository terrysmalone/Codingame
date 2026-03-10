namespace CodeRoyale;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;

// https://www.codingame.com/ide/puzzle/code-royale
internal sealed class Game
{        
    private const int _archerCost = 100;
    private const int _knightCost = 80;
    private const int _giantCost = 140;
        

    private readonly List<Site> _sites;
    private List<Unit> _playerUnits;
    private List<Unit> _enemyUnits;

    private HashSet<int> _archeryBarracksIds;
    private HashSet<int> knightBarracksIds;
    private HashSet<int> giantBarracksIds;
    private double distance;

    private List<StructureType> _structurePriorities = new List<StructureType>
    {
        StructureType.BarracksKnights,
        StructureType.Mine,
        StructureType.Mine,
        StructureType.Mine,
        StructureType.Mine,
        StructureType.BarracksKnights,
        //StructureType.BarracksArchers,        
        //StructureType.BarracksGiant,
    };

    internal int Gold { get; set; }
    internal int TouchedSite { get; set; }
    internal int NumberOfSites => _sites.Count;

    internal Game(List<Site> sites)
    {
        _sites = sites;

        _archeryBarracksIds = new HashSet<int>();
        knightBarracksIds = new HashSet<int>();
        giantBarracksIds = new HashSet<int>();
    }

    internal void UpdateSite(int siteId, int owner, int structureType, int gold, int maxMineSize)
    {
        Site site = _sites.Single(s => s.Id == siteId);

        site.Owner = owner;

        site.Structure = structureType switch
        {
            -1 => StructureType.Empty,
            0 => StructureType.Mine,
            1 => StructureType.Tower,
            2 => StructureType.BarracksUnknown,
            _ => StructureType.Empty
        };

        if(site.Owner == 0 && site.Structure == StructureType.BarracksUnknown)
        {
            if(_archeryBarracksIds.Contains(site.Id))
            {
                site.Structure = StructureType.BarracksArchers;
            }
            else if(knightBarracksIds.Contains(site.Id))
            {
                site.Structure = StructureType.BarracksKnights;
            }
            else if(giantBarracksIds.Contains(site.Id))
            {
                site.Structure = StructureType.BarracksGiant;
            }
        }
            
        if (site.Structure == StructureType.Tower)
        {
            site.IncrementTowerSize();
        }

        site.Gold = gold;
        site.MaxMineSize = maxMineSize;
    }

    internal void ClearUnits()
    {
        _playerUnits = new List<Unit>();
        _enemyUnits = new List<Unit>();
    }

    public void AddPlayerUnit(UnitType unitType, Point position, int health)
    {
        _playerUnits.Add(new Unit(unitType, position, health));
    }

    public void AddEnemyUnit(UnitType unitType, Point position, int health)
    {
        _enemyUnits.Add(new Unit(unitType, position, health));
    }

    public Tuple<string, string> GetAction()
    {
        //Debug.Sites(_sites);

        Unit playerQueen = _playerUnits.Single(u => u.Type == UnitType.Queen);
        Unit enemyQueen = _enemyUnits.Single(u => u.Type == UnitType.Queen);

        string trainAction = "TRAIN";

        string queenAction = DecideQueenAction();

        // GetAction train action
        List<Site> knightBarracks = _sites.Where(s => s.Owner == 0 && s.Structure == StructureType.BarracksKnights).ToList();
        List<Site> archerBarracks = _sites.Where(s => s.Owner == 0 && s.Structure == StructureType.BarracksArchers).ToList();
        List<Site> giantBarracks = _sites.Where(s => s.Owner == 0 && s.Structure == StructureType.BarracksGiant).ToList();

        List<Site> mines = _sites.Where(s => s.Owner == 0 && s.Structure == StructureType.Mine).ToList();


        UnitType idealUnit = GetIdealUnit();
            
        if(idealUnit == UnitType.Archer)
        {
            //Order barracks by closest to player queen
            archerBarracks = archerBarracks.OrderBy(b => CalculateDistance(playerQueen.Position, b.Position)).ToList();

            int goldLeft = Gold;

            foreach (Site barrack in archerBarracks)
            {
                if(goldLeft < _archerCost)
                {
                    break;
                }

                trainAction += $" {barrack.Id}";
                goldLeft -= _archerCost;
            }
        }
        else if (idealUnit == UnitType.Knight)
        {
            //Order barracks by closest to enemies queen
            knightBarracks = knightBarracks.OrderBy(b => CalculateDistance(enemyQueen.Position, b.Position)).ToList();

            int goldLeft = Gold;

            foreach (Site barrack in knightBarracks)
            {
                if(goldLeft < _knightCost)
                {
                    break;
                }

                trainAction += $" {barrack.Id}";
                goldLeft -= _knightCost;
            }
        }
        else if(giantBarracks.Any())
        {
            trainAction += $" {giantBarracks.First().Id}";
        }

        return new Tuple<string, string>(queenAction, trainAction);
    }

    private string DecideQueenAction()
    {        
        // First priority is building towers if the queen is being attacked and there are empty sites to build on.
        // Otherwise build to match _structurePriorities
        Unit queen = _playerUnits.Single(u => u.Type == UnitType.Queen);        

        string action = DecideWhetherToRunAway(queen);

        if (action != null)
        {
            return action;
        }

        StructureType nextSitePriority = GetNextSitePriority();

        int closestEmptySiteId = GetClosestEmptySite(queen.Position);
        Site closestSite = _sites.Single(s => s.Id == closestEmptySiteId);

        if (nextSitePriority != StructureType.Empty)
        {
            if (TouchedSite != closestEmptySiteId)
            {
                return $"MOVE {closestSite.Position.X} {closestSite.Position.Y}";
            }
            else
            { 
                UpdateBuiltSiteIDs(nextSitePriority, closestEmptySiteId);
                if( nextSitePriority == StructureType.Mine)
                {
                    closestSite.IncrementMineSize();
                }
                return $"BUILD {closestEmptySiteId} {CommandHelper.TranslateBuildingType(nextSitePriority)}";
            }
        }
        else
        {
            Debug.Sites(_sites);
            // Move towards the nearest mine that can be upgraded
            List<Site> upgradableMines = _sites.Where(s => s.Owner == 0 && s.Structure == StructureType.Mine && s.MineSize < s.MaxMineSize).ToList();
            if (upgradableMines.Any())
            {
                Site closestUpgradableMine = upgradableMines.OrderBy(m => CalculateDistance(queen.Position, m.Position)).First();

                Console.Error.WriteLine($"Closest upgradable mine is {closestUpgradableMine.Id} at {closestUpgradableMine.Position.X},{closestUpgradableMine.Position.Y} with size {closestUpgradableMine.MineSize} and max size {closestUpgradableMine.MaxMineSize}");
                if (TouchedSite != closestUpgradableMine.Id)
                {
                    return $"MOVE {closestUpgradableMine.Position.X} {closestUpgradableMine.Position.Y}";
                }
                else
                {
                    closestUpgradableMine.IncrementMineSize();
                    return $"BUILD {closestUpgradableMine.Id} {CommandHelper.TranslateBuildingType(StructureType.Mine)}";
                }
            }
        }

        // As a last resort, just build a new mmine on the closest empty site
        if (TouchedSite != closestEmptySiteId)
        {
            return $"MOVE {closestSite.Position.X} {closestSite.Position.Y}";
        }
        else
        {
            closestSite.IncrementMineSize();
            return $"BUILD {closestEmptySiteId} {CommandHelper.TranslateBuildingType(StructureType.Mine)}";
        }

        //return "WAIT";
    }

    private void UpdateBuiltSiteIDs(StructureType nextSitePriority, int id)
    {
        switch(nextSitePriority)
        {
            case StructureType.BarracksArchers:
                _archeryBarracksIds.Add(id);
                break;
            case StructureType.BarracksKnights:
                knightBarracksIds.Add(id);
                break;
            case StructureType.BarracksGiant:
                giantBarracksIds.Add(id);
                break;
        }
    }

    private StructureType GetNextSitePriority()
    {
        // console print all Sites in _sites
        Console.Error.WriteLine($"_sites: {string.Join(" ", _sites.Select(s => s.Structure))}");

        List<Site> knightBarracks = _sites.Where(s => s.Owner == 0 && s.Structure == StructureType.BarracksKnights).ToList();
        List<Site> archerBarracks = _sites.Where(s => s.Owner == 0 && s.Structure == StructureType.BarracksArchers).ToList();
        List<Site> giantBarracks = _sites.Where(s => s.Owner == 0 && s.Structure == StructureType.BarracksGiant).ToList();

        List<Site> mines = _sites.Where(s => s.Owner == 0 && s.Structure == StructureType.Mine).ToList();

        int knightBarracksCount = knightBarracks.Count;
        int archerBarracksCount = archerBarracks.Count;
        int giantBarracksCount = giantBarracks.Count;
        int minesCount = mines.Count;

        foreach (StructureType structureType in _structurePriorities)
        {
            Console.Error.WriteLine($"Checking structure type {structureType} with {knightBarracksCount} knight barracks, {archerBarracksCount} archer barracks, {giantBarracksCount} giant barracks and {minesCount} mines");
            if (structureType == StructureType.BarracksKnights)
            {
                if (knightBarracksCount != 0)
                {
                    knightBarracksCount--;
                }
                else
                {
                    return StructureType.BarracksKnights;
                }
            }

            if (structureType == StructureType.BarracksArchers)
            {
                if (archerBarracksCount != 0)
                {
                    archerBarracksCount--;
                }
                else
                {
                    return StructureType.BarracksArchers;
                }
            }

            if (structureType == StructureType.BarracksGiant)
            {
                if (giantBarracksCount != 0)
                {
                    giantBarracksCount--;
                }
                else
                {
                    return StructureType.BarracksGiant;
                }
            }

            if (structureType == StructureType.Mine)
            {
                if (minesCount != 0)
                {
                    minesCount--;
                }
                else
                {
                    return StructureType.Mine;
                }
            }
        }

        return StructureType.Empty;
    }

    private string DecideWhetherToRunAway(Unit queen)
    {
        Console.Error.WriteLine("Deciding whether to run away");
        Unit closestUnitToQueen = ClosestEnemyUnitToQueen(queen.Position);

        Console.Error.WriteLine($"Closest unit to queen is {closestUnitToQueen?.Type} at {closestUnitToQueen?.Position.X},{closestUnitToQueen?.Position.Y}");

        // Prioritise running away  
        int distanceThreshold = 200;
            
        if (closestUnitToQueen != null && CalculateDistance(queen.Position, closestUnitToQueen.Position) < distanceThreshold)
        {
            Console.Error.WriteLine($"Running away from {closestUnitToQueen.Type} at {closestUnitToQueen.Position.X},{closestUnitToQueen.Position.Y}");

            // Try to move towards archers
            if (_playerUnits.Any(u => u.Type == UnitType.Archer))
            {
                
                Point closestArcher = GetClosestUnit(queen, UnitType.Archer);
                Console.Error.WriteLine($"Running away towards archer at {closestArcher.X},{closestArcher.Y}");

                return $"MOVE {closestArcher.X} {closestArcher.Y}";
            }

            // Find the nearerst empty site and make it a tower
            if (_sites.Any(s => s.Structure == StructureType.Empty))
            {
                Site closestEmptySite = _sites.Where(s => s.Structure == StructureType.Empty).OrderBy(s => CalculateDistance(queen.Position, s.Position)).First();
                Console.Error.WriteLine($"Running away towards empty site at {closestEmptySite.Position.X},{closestEmptySite.Position.Y}");

                if(TouchedSite == closestEmptySite.Id)
                {
                    return $"BUILD {closestEmptySite.Id} TOWER";
                }

                return $"MOVE {closestEmptySite.Position.X} {closestEmptySite.Position.Y}";
            }

            //if (_sites.Any(u => u.Structure == StructureType.Tower))
            //{
            //    // Go to nearest tower
            //    var closestTower = GetClosestStructure(queen, StructureType.Tower, 0);

            //    return $"MOVE {closestTower.X} {closestTower.Y}";
            //}

            // Just run away
            return $"MOVE {queen.Position.X - (closestUnitToQueen.Position.X - queen.Position.X)} {queen.Position.Y - (closestUnitToQueen.Position.Y - queen.Position.Y)}";
        }

        return null;
    }

    private Point GetClosestUnit(Unit sourceUnit, UnitType unitType)
    {
        Point closestUnitPosition = new Point();
        double closestDistance = double.MaxValue;

        // Find closest unit
        foreach (Unit unit in _playerUnits.Where(u => u.Type == unitType))
        {
            distance = CalculateDistance(sourceUnit.Position, unit.Position);

            if (distance < closestDistance)
            {
                closestUnitPosition = unit.Position;
                closestDistance = distance;
            }
        }

        return closestUnitPosition;
    }
        
    private Point GetClosestStructure(Unit sourceUnit, StructureType structureType, int player)
    {
        Point closestStructurePosition = new Point();
        double closestDistance = double.MaxValue;

        IEnumerable<Site> sites = _sites.Where(s => s.Owner == player && s.Structure == structureType);
            
        foreach (Site site in sites)
        {
            distance = CalculateDistance(sourceUnit.Position, site.Position);

            if (distance < closestDistance)
            {
                closestStructurePosition = site.Position;
                closestDistance = distance;
            }
        }

        return closestStructurePosition;
    }

    private Unit ClosestEnemyUnitToQueen(Point queenPosition)
    {
        Unit closestUnit = null;
        double closestDistance = double.MaxValue;

        foreach (Unit knight in _enemyUnits.Where(u => u.Type == UnitType.Knight))
        {
            double distance = CalculateDistance(queenPosition, knight.Position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestUnit = knight;
            }
        }

        return closestUnit;
    }

    private int GetClosestEmptySite(Point queenPosition)
    {
        int closestId = -1;
        double closestDistance = double.MaxValue;

        foreach (Site site in _sites)
        {
            if(site.Structure != StructureType.Empty)
            {
                continue;
            }

            double distance = CalculateDistance(queenPosition, site.Position);

            if(distance < closestDistance)
            {
                closestDistance = distance;
                closestId = site.Id;
            }
        }

        return closestId;
    }

    private static double CalculateDistance(Point queenPosition, Point sitePosition)
    {
        return Math.Sqrt(Math.Pow(sitePosition.X - queenPosition.X, 2) + Math.Pow(sitePosition.Y - queenPosition.Y, 2)) ;
    }
        
    private UnitType GetIdealUnit()
    {
        return UnitType.Knight;
    }    
}

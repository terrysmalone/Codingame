﻿namespace CodeRoyale;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

// https://www.codingame.com/ide/puzzle/code-royale
internal sealed class Game
{
    private  bool _playAggressively = true;
        
    private const int _archerCost = 100;
    private const int _knightCost = 80;
    private const int _giantCost = 140;
        

    private readonly List<Site> _sites;
    private List<Unit> _playerUnits;
    private List<Unit> _enemyUnits;

    private List<int> _archeryBarracksIds;
    private List<int> knightBarracksIds;
    private List<int> giantBarracksIds;
    private double distance;

    internal int Gold { get; set; }
    internal int TouchedSite { get; set; }
    internal int NumberOfSites => _sites.Count;

    internal Game(List<Site> sites)
    {
        _sites = sites;

        _archeryBarracksIds = new List<int>();
        knightBarracksIds = new List<int>();
        giantBarracksIds = new List<int>();
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

        if (site.Structure == StructureType.Mine)
        {
            site.IncrementMineSize();
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
        //DebugAll();
        //DebugSites(false);

        Unit playerQueen = _playerUnits.Single(u => u.Type == UnitType.Queen);
        Unit enemyQueen = _enemyUnits.Single(u => u.Type == UnitType.Queen);

        string trainAction = "TRAIN";

        List<Site> knightBarracks = _sites.Where(s => s.Owner == 0 && s.Structure == StructureType.BarracksKnights).ToList();
        List<Site> archerBarracks = _sites.Where(s => s.Owner == 0 && s.Structure == StructureType.BarracksArchers).ToList();
        List<Site> giantBarracks = _sites.Where(s => s.Owner == 0 && s.Structure == StructureType.BarracksGiant).ToList();

        string queenAction = DecideQueenAction();

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
        int idealTowerCount = (_sites.Count - 9) / 2;
        int idealMineCount = 3;

        //Console.Error.WriteLine($"idealTowerCount:{idealTowerCount}");

        string queenAction = "WAIT";

        Unit queen = _playerUnits.Single(u => u.Type == UnitType.Queen);
        int closestEmptySiteId = GetClosestEmptySite(queen.Position);

        List<Site> knightBarracks = _sites.Where(s => s.Owner == 0 && s.Structure == StructureType.BarracksKnights).ToList();
        List<Site> archerBarracks = _sites.Where(s => s.Owner == 0 && s.Structure == StructureType.BarracksArchers).ToList();
        List<Site> giantBarracks = _sites.Where(s => s.Owner == 0 && s.Structure == StructureType.BarracksGiant).ToList();
        List<Site> towers = _sites.Where(s => s.Owner == 0 && s.Structure == StructureType.Tower).ToList();
        List<Site> mines = _sites.Where(s => s.Owner == 0 && s.Structure == StructureType.Mine).ToList();

        string action = DecideWhetherToRunAway(queen);

        if (action != null)
        {
            return action;
        }

        // If the queen is touching a mine that can be levelled up
        if (_sites.Where(s => s.Owner == 0 && s.Structure == StructureType.Mine).Select(s => s.Id).Contains(TouchedSite))
        {
            Site touchedMine = _sites.Single(s => s.Id == TouchedSite);

            if (touchedMine.MaxMineSize > touchedMine.MineSize)
            {
                return $"BUILD {touchedMine.Id} MINE";
            }
        }
            
        if (_sites.Where(s => s.Owner == 0 && s.Structure == StructureType.Tower).Select(s => s.Id).Contains(TouchedSite))
        {
            Site touchedTower = _sites.Single(s => s.Id == TouchedSite);

            if (touchedTower.TowerSize < 3 )
            {
                return $"BUILD {touchedTower.Id} TOWER";
            }
        }

        bool enemyHasTowers = _sites.Any(s => s.Owner != 0 && s.Structure == StructureType.Tower);

        if (_playAggressively)
        {
            if (closestEmptySiteId != -1)
            {
                Site closestSite = _sites.Single(s => s.Id == closestEmptySiteId);

                string buildingType = string.Empty;
                    
                if (mines.Count < idealMineCount && closestSite.Gold > 0)
                {
                    buildingType = "MINE";
                }
                else if (towers.Count < 1)
                {
                    buildingType = "TOWER";
                }
                else if (knightBarracks.Count < 1)
                {
                    buildingType = "BARRACKS-KNIGHT";
                    knightBarracksIds.Add(closestEmptySiteId);
                }
                else
                {
                    buildingType = "TOWER";
                }
                    
                if (!string.IsNullOrEmpty(buildingType))
                {
                    return $"BUILD {closestEmptySiteId} {buildingType}";
                }
                    
                if (towers.Count > 0)
                {
                    return $"MOVE {towers.First().Position.X} {towers.First().Position.Y}";
                }
            }
        }
        else
        {
            if (closestEmptySiteId != -1)
            {
                Site closestSite = _sites.Single(s => s.Id == closestEmptySiteId);

                string buildingType = string.Empty;

                if (archerBarracks.Count < 1)
                {
                    buildingType = "BARRACKS-ARCHER";
                    _archeryBarracksIds.Add(closestEmptySiteId);
                }
                else if (mines.Count < idealMineCount && closestSite.Gold > 0)
                {
                    buildingType = "MINE";
                }
                else if (knightBarracks.Count < 1)
                {
                    buildingType = "BARRACKS-KNIGHT";
                    knightBarracksIds.Add(closestEmptySiteId);
                }
                else if (giantBarracks.Count < 1 && enemyHasTowers)
                {
                    buildingType = "BARRACKS-GIANT";
                    giantBarracksIds.Add(closestEmptySiteId);
                }
                else if (towers.Count < idealTowerCount)
                {
                    buildingType = "TOWER";
                }
                else
                {
                    // Find closest mine
                    buildingType = "MINE";

                    // Upgrade it
                    closestEmptySiteId = mines.First().Id;
                }

                if (!string.IsNullOrEmpty(buildingType))
                {
                    return $"BUILD {closestEmptySiteId} {buildingType}";
                }
            }

            //queenAction = $"MOVE 0 0";

            //Console.Error.WriteLine($"Queen move to {towers.First().Position.X},{towers.First().Position.Y}");

            if (towers.Count > 0)
            {
                return $"MOVE {towers.First().Position.X} {towers.First().Position.Y}";
            }
        }
            
        return $"WAIT"; 
    }

    private string DecideWhetherToRunAway(Unit queen)
    {
        Unit closestUnitToQueen = ClosestEnemyUnitToQueen(queen.Position);

        // Prioritise running away  
        int distanceThreshold = 200;
            
        if (closestUnitToQueen != null && CalculateDistance(queen.Position, closestUnitToQueen.Position) < distanceThreshold)
        {
            // Try to move towards archers
            if (_playerUnits.Any(u => u.Type == UnitType.Archer))
            {
                Point closestArcher = GetClosestUnit(queen, UnitType.Archer);
                    
                return $"MOVE {closestArcher.X} {closestArcher.Y}";
            }
                
            // if (_sites.Any(u => u.Structure == StructureType.Tower))
            // {
            //     // Go to nearest tower
            //     var closestTower = GetClosestStructure(queen, StructureType.Tower, 0);
            //     
            //     return $"MOVE {closestTower.X} {closestTower.Y}";
            // }
                
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
        int numberOfGiants = _playerUnits.Count(u => u.Type == UnitType.Giant);
            
        if (_playAggressively)
        {
            // Just make knights

            if (_sites.Count(s => s.Owner == 0 && s.Structure == StructureType.BarracksGiant) > 0
                && _sites.Count(s => s.Owner != 0 && s.Structure == StructureType.Tower) > 0
                && numberOfGiants == 0)
            {
                return UnitType.Giant;
            }
                
            return UnitType.Knight;
        }
        else
        {
            // We want a proportion of
            // 4 knights to
            // 2 archers to
            // 1 giant
            int idealKnightProportion = 2;
            int idealArcherProportion = 4;
            int idealGiantProportion = 1;

            int numberOfKnights = _playerUnits.Count(u => u.Type == UnitType.Knight);
            int numberOfArchers = _playerUnits.Count(u => u.Type == UnitType.Archer);

            if (numberOfArchers <= 2)
            {
                return UnitType.Archer;
            }

            if (numberOfKnights <= 1)
            {
                return UnitType.Knight;
            }

            if (numberOfGiants <= 0)
            {
                return UnitType.Giant;
            }

            while (true)
            {
                numberOfKnights -= idealKnightProportion;
                numberOfArchers -= idealArcherProportion;
                numberOfGiants -= idealGiantProportion;

                if (numberOfArchers <= 2)
                {
                    //Console.Error.WriteLine($"Archer chosen-Counts:{numberOfKnights},{numberOfArchers},{numberOfGiants}");
                    return UnitType.Archer;
                }

                if (numberOfKnights <= 1)
                {
                    //Console.Error.WriteLine($"Knight chosen-Counts:{numberOfKnights},{numberOfArchers},{numberOfGiants}");
                    return UnitType.Knight;
                }

                if (numberOfGiants <= 0)
                {
                    //Console.Error.WriteLine($"Giant chosen-Counts:{numberOfKnights},{numberOfArchers},{numberOfGiants}");
                    return UnitType.Giant;
                }
            }
        }
    }

    private void DebugAll()
    {
        DebugSites(false);
        DebugPlayerUnits();
        DebugEnemyUnits();
    }

    private void DebugSites(bool showEmptySites)
    {
        Console.Error.WriteLine("Sites");
        Console.Error.WriteLine("------");

        foreach (Site site in _sites)
        {
            if(!showEmptySites && site.Structure == StructureType.Empty)
            {
                continue;
            }

            Console.Error.WriteLine($"SiteId:{site.Id}");
            Console.Error.WriteLine($"Structure:{site.Structure}");
            Console.Error.WriteLine($"Owner:{site.Owner}");
            Console.Error.WriteLine($"Position:{site.Position}");
            Console.Error.WriteLine($"Radius:{site.Radius}");
            Console.Error.WriteLine("------");
        }

        Console.Error.WriteLine();
    }

    private void DebugPlayerUnits()
    {
        Console.Error.WriteLine("Player Units");
        Console.Error.WriteLine("------");

        foreach (Unit unit in _playerUnits)
        {
            Console.Error.WriteLine($"Type:{unit.Type}");
            Console.Error.WriteLine($"Position:{unit.Position.X},{unit.Position.Y}");
            Console.Error.WriteLine($"Position:{unit.Health}");
            Console.Error.WriteLine("------");
        }

        Console.Error.WriteLine();
    }

    private void DebugEnemyUnits()
    {
        Console.Error.WriteLine("Enemy Units");
        Console.Error.WriteLine("------");

        foreach (Unit unit in _enemyUnits)
        {
            Console.Error.WriteLine($"Type:{unit.Type}");
            Console.Error.WriteLine($"Position:{unit.Position.X},{unit.Position.Y}");
            Console.Error.WriteLine($"Position:{unit.Health}");
            Console.Error.WriteLine("------");
        }

        Console.Error.WriteLine();
    }
}
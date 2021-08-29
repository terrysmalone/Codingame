using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Transactions;

namespace CodeRoyale
{
    class Player
    {
        static void Main(string[] args)
        {
            var sites = GetSites();
            var game = new Game(sites);

            // game loop
            while (true)
            {
                var inputs = Console.ReadLine().Split(' ');

                game.Gold = int.Parse(inputs[0]);
                game.TouchedSite = int.Parse(inputs[1]); // -1 if none

                UpdateSites(game);

                UpdateUnits(game);

                var actions = game.GetAction();

                // Write an action using Console.WriteLine()
                // To debug: Console.Error.WriteLine("Debug messages...");


                // First line: A valid queen action
                // Second line: A set of training instructions
                Console.WriteLine(actions.Item1);
                Console.WriteLine(actions.Item2);
            }
        }

        private static List<Site> GetSites()
        {
            var sites = new List<Site>();

            var  numSites = int.Parse(Console.ReadLine());

            for (var i = 0; i < numSites; i++)
            {
                var siteInfo = Console.ReadLine().Split(' ');

                sites.Add(new Site(int.Parse(siteInfo[0]),
                                       new Point(int.Parse(siteInfo[1]), int.Parse(siteInfo[2])),
                                       int.Parse(siteInfo[3])));
            }

            return sites;
        }

        private static void UpdateSites(Game game)
        {
            for (var i = 0; i < game.NumberOfSites; i++)
            {
                var inputs = Console.ReadLine().Split(' ');
                var siteId = int.Parse(inputs[0]);
                var gold = int.Parse(inputs[1]); 
                var maxMineSize = int.Parse(inputs[2]);
                var structureType = int.Parse(inputs[3]); // -1 = No structure, 2 = Barracks
                var owner = int.Parse(inputs[4]); // -1 = No structure, 0 = Friendly, 1 = Enemy
                var param1 = int.Parse(inputs[5]);
                var param2 = int.Parse(inputs[6]);

                game.UpdateSite(siteId,
                                owner,
                                structureType,
                                gold,
                                maxMineSize);
            }
        }

        private static void UpdateUnits(Game game)
        {
            game.ClearUnits();

            var numberOfUnits = int.Parse(Console.ReadLine());

            for (var i = 0; i < numberOfUnits; i++)
            {
                var inputs = Console.ReadLine().Split(' ');

                var position = new Point(int.Parse(inputs[0]), int.Parse(inputs[1]));
                var owner = int.Parse(inputs[2]);

                var unitType = int.Parse(inputs[3]) switch
                {
                    -1 => UnitType.Queen,
                    0 => UnitType.Knight,
                    1 => UnitType.Archer,
                    2 => UnitType.Giant,
                    _ => throw new ArgumentException()
                };

                var health = int.Parse(inputs[4]);

                if(owner == 0)
                {
                    game.AddPlayerUnit(unitType,
                                       position,
                                       health);
                }
                else
                {
                    game.AddEnemyUnit(unitType,
                                       position,
                                       health);
                }
            }
        }
    }

    internal sealed class Game
    {
        private const int _archerCost = 100;
        private const int _knightCost = 80;
        private const int _giantCost = 140;
        

        private List<Site> _sites;
        private List<Unit> _playerUnits;
        private List<Unit> _enemyUnits;

        private List<int> _archeryBarracksIds;
        private List<int> knightBarracksIds;
        private List<int> giantBarracksIds;

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
            var site = _sites.Single(s => s.Id == siteId);

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

            var playerQueen = _playerUnits.Single(u => u.Type == UnitType.Queen);
            var enemyQueen = _enemyUnits.Single(u => u.Type == UnitType.Queen);
            
            var trainAction = "TRAIN";

            var knightBarracks = _sites.Where(s => s.Owner == 0 && s.Structure == StructureType.BarracksKnights).ToList();
            var archerBarracks = _sites.Where(s => s.Owner == 0 && s.Structure == StructureType.BarracksArchers).ToList();
            var giantBarracks = _sites.Where(s => s.Owner == 0 && s.Structure == StructureType.BarracksGiant).ToList();
            
            var queenAction = DecideQueenAction();
            
            var idealUnit = GetIdealUnit();
            
            if(idealUnit == UnitType.Archer)
            {
                //Order barracks by closest to player queen
                archerBarracks = archerBarracks.OrderBy(b => Distance(playerQueen.Position, b.Position)).ToList();

                var goldLeft = Gold;

                foreach (var barrack in archerBarracks)
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
                knightBarracks = knightBarracks.OrderBy(b => Distance(enemyQueen.Position, b.Position)).ToList();

                var goldLeft = Gold;

                foreach (var barrack in knightBarracks)
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
            var idealTowerCount = (_sites.Count - 9) / 2; 
            var idealMineCount = 4; 
            
            //Console.Error.WriteLine($"idealTowerCount:{idealTowerCount}");
                
            var queenAction = "WAIT";

            var queen = _playerUnits.Single(u => u.Type == UnitType.Queen);
            var closestEmptySiteId = GetClosestEmptySite(queen.Position);

            var knightBarracks = _sites.Where(s => s.Owner == 0 && s.Structure == StructureType.BarracksKnights).ToList();
            var archerBarracks = _sites.Where(s => s.Owner == 0 && s.Structure == StructureType.BarracksArchers).ToList();
            var giantBarracks = _sites.Where(s => s.Owner == 0 && s.Structure == StructureType.BarracksGiant).ToList();
            var towers = _sites.Where(s => s.Owner == 0 && s.Structure == StructureType.Tower).ToList();
            var mines = _sites.Where(s => s.Owner == 0 && s.Structure == StructureType.Mine).ToList();
            
            // Prioritise running away
            var closestUnitToQueen = ClosestUnitToQueen(queen.Position);

            var distanceThreshold = 200;
            
            if (closestUnitToQueen != null && Distance(queen.Position, closestUnitToQueen.Position) < distanceThreshold)
            {
                Console.Error.WriteLine($"Too close. Moving to :{queen.Position.X - (closestUnitToQueen.Position.X - queen.Position.X)}, {queen.Position.Y - (closestUnitToQueen.Position.Y - queen.Position.Y)}");
                return $"MOVE {queen.Position.X - (closestUnitToQueen.Position.X - queen.Position.X)} {queen.Position.Y - (closestUnitToQueen.Position.Y - queen.Position.Y)}";
            }

            // If the queen is touching a mine that can be levelled up
            if (_sites.Where(s => s.Owner == 0 && s.Structure == StructureType.Mine).Select(s => s.Id).Contains(TouchedSite))
            {
                var touchedMine = _sites.Single(s => s.Id == TouchedSite);

                if (touchedMine.MaxMineSize > touchedMine.MineSize)
                {
                    return $"BUILD {touchedMine.Id} MINE";
                }
            }

            if (closestEmptySiteId != -1)
            {
                var closestSite = _sites.Single(s => s.Id == closestEmptySiteId);
                
                var buildingType = string.Empty;
                var enemyHasTowers = giantBarracks.Count < 1 && _sites.Any(s => s.Owner != 0 && s.Structure == StructureType.Tower);
                
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
            else
            {
                return $"WAIT";
            }
        }

        private Unit ClosestUnitToQueen(Point queenPosition)
        {
            Unit closestUnit = null;
            var closestDistance = double.MaxValue;

            foreach (var knight in _enemyUnits.Where(u => u.Type == UnitType.Knight))
            {
                var distance = Distance(queenPosition, knight.Position);

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
            var closestId = -1;
            var closestDistance = double.MaxValue;

            foreach (var site in _sites)
            {
                if(site.Structure != StructureType.Empty)
                {
                    continue;
                }

                var distance = Distance(queenPosition, site.Position);

                if(distance < closestDistance)
                {
                    closestDistance = distance;
                    closestId = site.Id;
                }
            }

            return closestId;
        }

        private static double Distance(Point queenPosition, Point sitePosition)
        {
            return Math.Sqrt(Math.Pow(sitePosition.X - queenPosition.X, 2) + Math.Pow(sitePosition.Y - queenPosition.Y, 2)) ;
        }
        
        private UnitType GetIdealUnit()
        {
            // We want a proportion of
            // 4 knights to
            // 2 archers to
            // 1 giant
            var idealKnightProportion = 2;
            var idealArcherProportion = 4;
            var idealGiantProportion = 1;
            
            var numberOfKnights = _playerUnits.Count(u => u.Type == UnitType.Knight);
            var numberOfArchers = _playerUnits.Count(u => u.Type == UnitType.Archer);
            var numberOfGiants = _playerUnits.Count(u => u.Type == UnitType.Giant);

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

            foreach (var site in _sites)
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

            foreach (var unit in _playerUnits)
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

            foreach (var unit in _enemyUnits)
            {
                Console.Error.WriteLine($"Type:{unit.Type}");
                Console.Error.WriteLine($"Position:{unit.Position.X},{unit.Position.Y}");
                Console.Error.WriteLine($"Position:{unit.Health}");
                Console.Error.WriteLine("------");
            }

            Console.Error.WriteLine();
        }
    }

    internal sealed class Site
    {
        internal int Id { get; }
        internal Point Position { get; }
        internal int Radius { get; }
        internal StructureType Structure { get; set; }
        internal int Owner { get; set; }
        public int Gold { get; set; }
        public int MaxMineSize { get; set; }
        
        public int MineSize { get; private set; }

        internal Site(int id, Point position, int radius)
        {
            Id = id;
            Position = position;
            Radius = radius;

            Structure = StructureType.Empty;
        }

        public void IncrementMineSize()
        {
            MineSize++;
        }
    }

    internal sealed class Unit
    {
        internal UnitType Type { get; }
        internal Point Position { get; }
        internal int Health { get; }
        internal Unit(UnitType type,
                      Point position,
                      int health)
        {
            Type = type;
            Position = position;
            Health = health;
        }
    }

    internal enum StructureType
    {
        BarracksArchers,
        BarracksKnights,
        BarracksGiant,
        BarracksUnknown,
        Empty,
        Tower,
        Mine
    }

    internal enum UnitType
    {
        Queen,
        Knight,
        Archer,
        Giant
    }
}

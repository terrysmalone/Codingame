﻿using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

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
                var input = Console.ReadLine();

                Console.Error.WriteLine(input);
                var inputs = input.Split(' ');
                var siteId = int.Parse(inputs[0]);
                var ignore1 = int.Parse(inputs[1]); // used in future leagues
                var ignore2 = int.Parse(inputs[2]); // used in future leagues
                var structureType = int.Parse(inputs[3]); // -1 = No structure, 2 = Barracks
                var owner = int.Parse(inputs[4]); // -1 = No structure, 0 = Friendly, 1 = Enemy
                var param1 = int.Parse(inputs[5]);
                var param2 = int.Parse(inputs[6]);

                game.UpdateSite(siteId,
                                owner,
                                structureType);
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

                UnitType unitType = int.Parse(inputs[3]) switch
                {
                    -1 => UnitType.Queen,
                    0 => UnitType.Knight,
                    1 => UnitType.Archer,
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
        private List<Site> _sites;
        private List<Unit> _playerUnits;
        private List<Unit> _enemyUnits;

        internal int Gold { get; set; }
        internal int TouchedSite { get; set; }
        internal int NumberOfSites => _sites.Count;

        internal Game(List<Site> sites)
        {
            _sites = sites;
        }

        internal void UpdateSite(int siteId, int owner, int structureType)
        {
            var site = _sites.Single(s => s.Id == siteId);

            site.Owner = owner;

            site.Structure = structureType switch
            {
                -1 => StructureType.Empty,
                2 => StructureType.Barracks,
                _ => StructureType.Empty
            };
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

            var queen = _playerUnits.Single(u => u.Type == UnitType.Queen);

            var queenAction = "WAIT";
            var trainAction = "TRAIN";

            // Build a barracks
            var closestEmptySiteId = GetClosestEmptySite(queen.Position);
            queenAction = $"BUILD {closestEmptySiteId} BARRACKS-KNIGHT";

            // Train units
            var playerBarracks = _sites.Where(s => s.Owner == 0 && s.Structure == StructureType.Barracks).ToList();

            Console.Error.WriteLine($"playerBarracks:{playerBarracks.Count}");

            var goldLeft = Gold;

            foreach (var barrack in playerBarracks)
            {
                if(goldLeft < 80)
                {
                    break;
                }

                trainAction += $" {barrack.Id}";
                goldLeft -= 80;
            }

            return new Tuple<string, string>(queenAction, trainAction);
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

        internal Site(int id, Point position, int radius)
        {
            Id = id;
            Position = position;
            Radius = radius;

            Structure = StructureType.Empty;
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
        Empty,
        Barracks
    }

    internal enum UnitType
    {
        Queen,
        Knight,
        Archer
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;

namespace CodeRoyale
{
    internal sealed class Player
    {
        static void Main(string[] args)
        {
            var sites = GetSites();
            var game = new Game(sites);

            // game loop
            while (true)
            {
                // ReSharper disable once PossibleNullReferenceException
                var inputs = Console.ReadLine().Split(' ');

                game.Gold = int.Parse(inputs[0]);
                game.TouchedSite = int.Parse(inputs[1]); // -1 if none

                UpdateSites(game);
                UpdateUnits(game);

                var (queenAction, trainingAction) = game.GetAction();

                // First line: A valid queen action
                // Second line: A set of training instructions
                Console.WriteLine(queenAction);
                Console.WriteLine(trainingAction);
            }
        }

        private static List<Site> GetSites()
        {
            var sites = new List<Site>();

            // ReSharper disable once AssignNullToNotNullAttribute
            var  numSites = int.Parse(Console.ReadLine());

            for (var i = 0; i < numSites; i++)
            {
                // ReSharper disable once PossibleNullReferenceException
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
                // ReSharper disable once PossibleNullReferenceException
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

            // ReSharper disable once AssignNullToNotNullAttribute
            var numberOfUnits = int.Parse(Console.ReadLine());

            for (var i = 0; i < numberOfUnits; i++)
            {
                // ReSharper disable once PossibleNullReferenceException
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
}

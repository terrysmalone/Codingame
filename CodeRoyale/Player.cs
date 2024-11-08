namespace CodeRoyale;

using System;
using System.Collections.Generic;
using System.Drawing;

internal sealed class Player
{
    static void Main(string[] args)
    {
        List<Site> sites = GetSites();
        Game game = new Game(sites);

        // game loop
        while (true)
        {
            // ReSharper disable once PossibleNullReferenceException
            string[] inputs = Console.ReadLine().Split(' ');

            game.Gold = int.Parse(inputs[0]);
            game.TouchedSite = int.Parse(inputs[1]); // -1 if none

            UpdateSites(game);
            UpdateUnits(game);

            (string queenAction, string trainingAction) = game.GetAction();

            // First line: A valid queen action
            // Second line: A set of training instructions
            Console.WriteLine(queenAction);
            Console.WriteLine(trainingAction);
        }
    }

    private static List<Site> GetSites()
    {
        List<Site> sites = new List<Site>();

        // ReSharper disable once AssignNullToNotNullAttribute
        int numSites = int.Parse(Console.ReadLine());

        for (int i = 0; i < numSites; i++)
        {
            // ReSharper disable once PossibleNullReferenceException
            string[] siteInfo = Console.ReadLine().Split(' ');

            sites.Add(new Site(int.Parse(siteInfo[0]),
                                    new Point(int.Parse(siteInfo[1]), int.Parse(siteInfo[2])),
                                    int.Parse(siteInfo[3])));
        }

        return sites;
    }

    private static void UpdateSites(Game game)
    {
        for (int i = 0; i < game.NumberOfSites; i++)
        {
            // ReSharper disable once PossibleNullReferenceException
            string[] inputs = Console.ReadLine().Split(' ');
            int siteId = int.Parse(inputs[0]);
            int gold = int.Parse(inputs[1]);
            int maxMineSize = int.Parse(inputs[2]);
            int structureType = int.Parse(inputs[3]); // -1 = No structure, 2 = Barracks
            int owner = int.Parse(inputs[4]); // -1 = No structure, 0 = Friendly, 1 = Enemy
            int param1 = int.Parse(inputs[5]);
            int param2 = int.Parse(inputs[6]);

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
        int numberOfUnits = int.Parse(Console.ReadLine());

        for (int i = 0; i < numberOfUnits; i++)
        {
            // ReSharper disable once PossibleNullReferenceException
            string[] inputs = Console.ReadLine().Split(' ');

            Point position = new Point(int.Parse(inputs[0]), int.Parse(inputs[1]));
            int owner = int.Parse(inputs[2]);

            UnitType unitType = int.Parse(inputs[3]) switch
            {
                -1 => UnitType.Queen,
                0 => UnitType.Knight,
                1 => UnitType.Archer,
                2 => UnitType.Giant,
                _ => throw new ArgumentException()
            };

            int health = int.Parse(inputs[4]);

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
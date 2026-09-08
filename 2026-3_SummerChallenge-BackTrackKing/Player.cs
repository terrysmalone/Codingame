using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace BackTrackKing;

/**
 * Connect towns with your train tracks and disrupt the opponent's.
 **/
class Player
{
    static void Main(string[] args)
    {
        string[] inputs;
        int myId = int.Parse(Console.ReadLine()); // 0 or 1

        var game = new Game(myId);

        int width = int.Parse(Console.ReadLine()); // map size
        int height = int.Parse(Console.ReadLine());

        InitialiseMap(game, width, height);

        InitialiseTowns(game);

        // game loop
        while (true)
        {
            int myScore = int.Parse(Console.ReadLine());
            int foeScore = int.Parse(Console.ReadLine());

            game.SetMyScore(myScore);
            game.SetOpponentScore(foeScore);

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    int tracksOwner = int.Parse(inputs[0]);
                    

                    int instability = int.Parse(inputs[1]); // region inked (destroyed) when this >= 3.
                    bool inked = inputs[2] != "0"; // true if region is destroyed.

                    game.UpdateCell(j, i, tracksOwner, instability, inked);

                    string partOfActiveConnections = inputs[3]; // if this cell is part of one or more railway connections, this will be town ids (separated by -) in a list separated by commas. e.g. 0-1,1-2,1-3. "x" otherwise.
                }
            }

            string actions = game.CalculateActions();

            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");


            // AUTOPLACE x1 y1 x2 | PLACE_TRACKS x y | DISRUPT regionId | MESSAGE text
            Console.WriteLine(actions);
        }
    }

    private static void InitialiseMap(Game game, int width, int height)
    {
        var map = new Map(width, height);

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                var inputs = Console.ReadLine().Split(' ');
                int regionId = int.Parse(inputs[0]);
                int type = int.Parse(inputs[1]); // 0 (PLAINS), 1 (RIVER), 2 (MOUNTAIN), 3 (POI)

                map.SetCell(j, i, (CellType)type, regionId);
                game.InitialiseCellToRegion(j, i, regionId);
            }
        }

        game.SetMap(map);
    }

    private static void InitialiseTowns(Game game)
    {
        List<Town> towns = new List<Town>();
        int townCount = int.Parse(Console.ReadLine());
        for (int i = 0; i < townCount; i++)
        {
            var inputs = Console.ReadLine().Split(' ');
            int townId = int.Parse(inputs[0]);
            int townX = int.Parse(inputs[1]);
            int townY = int.Parse(inputs[2]);
            string desiredConnections = inputs[3]; // comma-separated town ids e.g. 0,1,2,3

            List<int> desiredConnectionsList = new List<int>();
            if (desiredConnections != "x")
            {
                desiredConnectionsList = desiredConnections.Split(',').Select(int.Parse).ToList();
            }

            var town = new Town(townId, townX, townY, desiredConnectionsList);
            towns.Add(town);
        }

        game.SetTowns(towns);
    }
}

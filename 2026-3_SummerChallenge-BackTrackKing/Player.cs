using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

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

        int width = int.Parse(Console.ReadLine()); // map size
        int height = int.Parse(Console.ReadLine());

        var game = new Game(myId, width, height);

        InitialiseMap(game, width, height);

        InitialiseTowns(game);

        // game loop
        while (true)
        {
            Logger.StartRoundStopwatch();
            Logger.LogTime($"Starting round set up");

            int myScore = int.Parse(Console.ReadLine());
            int foeScore = int.Parse(Console.ReadLine());

            game.SetMyScore(myScore);
            game.SetOpponentScore(foeScore);

            game.ResetRegions();

            Dictionary<string, (int, int)> connectionScores = new Dictionary<string, (int, int)>();
            int myPoints = 0;
            int opponentPoints = 0;

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    int tracksOwner = int.Parse(inputs[0]); // -1 if this cell has no track. 2 if neutral



                    int instability = int.Parse(inputs[1]); // region inked (destroyed) when this >= 3.
                    bool inked = inputs[2] != "0"; // true if region is destroyed.

                    string partOfActiveConnections = inputs[3]; // if this cell is part of one or more railway connections, this will be town ids (separated by -) in a list separated by commas. e.g. 0-1,1-2,1-3. "x" otherwise.

                    string[]? connections = null;

                    if (partOfActiveConnections != "x")
                    {
                        connections = partOfActiveConnections.Split(',');
                        foreach (var connection in connections)
                        {
                             var towns = connection.Split('-');
                            // int townAId = int.Parse(towns[0]);
                            // int townBId = int.Parse(towns[1]);


                            if (tracksOwner == 2)
                            {
                                //myPoints++;
                                //opponentPoints++;
                                // If towns already exists, increment item 1 and2
                                if (connectionScores.ContainsKey(connection))
                                {
                                    connectionScores[connection] = (connectionScores[connection].Item1 + 1, connectionScores[connection].Item2 + 1);
                                }
                                else
                                {
                                    connectionScores[connection] = (1, 1);
                                }
                            }
                            else if (tracksOwner == myId)
                            {
                                myPoints++;
                                // If towns already exists, increment item 1
                                if (connectionScores.ContainsKey(connection))
                                {
                                    connectionScores[connection] = (connectionScores[connection].Item1 + 1, connectionScores[connection].Item2);
                                }
                                else
                                {
                                    connectionScores[connection] = (1, 0);
                                }
                            }
                            else if (tracksOwner != -1)
                            {
                                opponentPoints++;
                                // If towns already exists, increment item 2
                                if (connectionScores.ContainsKey(connection))
                                {
                                    connectionScores[connection] = (connectionScores[connection].Item1, connectionScores[connection].Item2 + 1);
                                }
                                else
                                {
                                    connectionScores[connection] = (0, 1);
                                }
                            }

                        }
                    }


                    game.UpdateCell(j, i, tracksOwner, instability, inked, connections);
                }
            }

            Logger.LogTime($"Round set up complete");

            Logger.Message($"Round points - Me: {myPoints}, Opponent: {opponentPoints}");

            string actions = game.CalculateActions();

            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");

            Logger.LogTime($"Round end");
            Logger.EndRoundStopwatch();

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

            game.AddTownToRegion(townId, townX, townY);
        }

        game.SetTowns(towns);
    }
}

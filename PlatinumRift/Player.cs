﻿using System;

namespace PlatinumRift
{
    public class Player
    {
        static void Main(string[] args)
        {
            string[] inputs;
            inputs = Console.ReadLine().Split(' ');
            int playerCount = int.Parse(inputs[0]); // the amount of players (2 to 4)
            int myId = int.Parse(inputs[1]); // my player ID (0, 1, 2 or 3)
            int zoneCount = int.Parse(inputs[2]); // the amount of zones on the map
            int linkCount = int.Parse(inputs[3]); // the amount of links between all zones
            for (int i = 0; i < zoneCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int zoneId = int.Parse(inputs[0]); // this zone's ID (between 0 and zoneCount-1)
                int platinumSource = int.Parse(inputs[1]); // the amount of Platinum this zone can provide per game turn
            }
            for (int i = 0; i < linkCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int zone1 = int.Parse(inputs[0]);
                int zone2 = int.Parse(inputs[1]);
            }

            // game loop
            while (true)
            {
                int platinum = int.Parse(Console.ReadLine()); // my available Platinum
                for (int i = 0; i < zoneCount; i++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    int zId = int.Parse(inputs[0]); // this zone's ID
                    int ownerId = int.Parse(inputs[1]); // the player who owns this zone (-1 otherwise)
                    int podsP0 = int.Parse(inputs[2]); // player 0's PODs on this zone
                    int podsP1 = int.Parse(inputs[3]); // player 1's PODs on this zone
                    int podsP2 = int.Parse(inputs[4]); // player 2's PODs on this zone (always 0 for a two player game)
                    int podsP3 = int.Parse(inputs[5]); // player 3's PODs on this zone (always 0 for a two or three player game)
                }

                // Write an action using Console.WriteLine()
                // To debug: Console.Error.WriteLine("Debug messages...");


                // first line for movement commands, second line for POD purchase (see the protocol in the statement for details)
                Console.WriteLine("WAIT");
                Console.WriteLine("1 73");
            }
        }
    }
}

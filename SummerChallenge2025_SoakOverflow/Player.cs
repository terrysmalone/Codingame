using System.Net.WebSockets;
using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace SummerChallenge2025_SoakOverflow;

/**
 * Win the water fight by controlling the most territory, or out-soak your opponent!
 **/
class Player
{
 

    static void Main(string[] args)
    {
        string[] inputs;
        int myId = int.Parse(Console.ReadLine()); // Your player id (0 or 1)
        int agentDataCount = int.Parse(Console.ReadLine()); // Total number of agents in the game

        var game = new Game(myId);

        for (int i = 0; i < agentDataCount; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int agentId = int.Parse(inputs[0]); // Unique identifier for this agent
            int player = int.Parse(inputs[1]); // Player id of this agent
            int shootCooldown = int.Parse(inputs[2]); // Number of turns between each of this agent's shots
            int optimalRange = int.Parse(inputs[3]); // Maximum manhattan distance for greatest damage output
            int soakingPower = int.Parse(inputs[4]); // Damage output within optimal conditions
            int splashBombs = int.Parse(inputs[5]); // Number of splash bombs this can throw this game

            game.AddAgent(agentId, player, shootCooldown, optimalRange, soakingPower, splashBombs);
        }

        inputs = Console.ReadLine().Split(' ');
        int width = int.Parse(inputs[0]); // Width of the game map
        int height = int.Parse(inputs[1]); // Height of the game map

        game.SetGameSize(width, height);

        for (int i = 0; i < height; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            for (int j = 0; j < width; j++)
            {
                int x = int.Parse(inputs[3 * j]);// X coordinate, 0 is left edge
                int y = int.Parse(inputs[3 * j + 1]);// Y coordinate, 0 is top edge
                int tileType = int.Parse(inputs[3 * j + 2]);
            }
        }

        int count = 1;

        // game loop
        while (true)
        {
            // This is a workaround for a weird bug I found. Hopefully They'll fix it
            if (count > 1)
            {
                Console.ReadLine();
            }
            Console.Error.WriteLine("Starting new game loop...");
            game.MarkAllAgentsForCulling();

            int agentCount = int.Parse(Console.ReadLine()); // Total number of agents still in the game
            Console.Error.WriteLine($"Processing {agentCount} agents...");
            for (int i = 0; i < agentCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                Console.Error.WriteLine($"Processing agent data: {string.Join(", ", inputs)}");
                int agentId = int.Parse(inputs[0]);
                int x = int.Parse(inputs[1]);
                int y = int.Parse(inputs[2]);
                int cooldown = int.Parse(inputs[3]); // Number of turns before this agent can shoot
                int splashBombs = int.Parse(inputs[4]);
                int wetness = int.Parse(inputs[5]); // Damage (0-100) this agent has taken

                game.UpdateAgent(agentId, x, y, cooldown, splashBombs, wetness);
            }

            game.DestroyMarkedAgents();

            List<string> moves = game.GetMoves();
            foreach (var move in moves)
            {
                Console.WriteLine(move);
            }

            count++;
        }
    }
}

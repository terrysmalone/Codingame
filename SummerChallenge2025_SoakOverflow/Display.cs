﻿

namespace SummerChallenge2025_SoakOverflow;

internal static class Display
{
    internal static void AgentHistories(List<Agent> playerAgents)
    {
        foreach (Agent agent in playerAgents)
        {
            Console.Error.WriteLine($"Agent {agent.Id}  MoveList: [{string.Join(", ", agent.MoveList)}]");

        }
    }

    internal static void Map(double[,] map)
    {
        for (int y = 0; y < map.GetLength(1); y++)
        {
            for (int x = 0; x < map.GetLength(0); x++)
            {
                Console.Error.Write($"{map[x, y]:F2} ");
            }
            Console.Error.WriteLine();
        }
    }

    internal static void Map(int[,] map)
    {
        for (int y = 0; y < map.GetLength(1); y++)
        {
            for (int x = 0; x < map.GetLength(0); x++)
            {
                Console.Error.Write($"{map[x, y]} ");
            }
            Console.Error.WriteLine();
        }
    }

    internal static void Sources(List<Agent> agents)
    {
        Console.Error.WriteLine("Move sources");

        foreach (Agent agent in agents)
        {
            Console.Error.WriteLine($"Agent {agent.Id} - Move: {agent.MoveIntention.Source} - Action:{agent.ActionIntention.Source}");
        }
    }
}
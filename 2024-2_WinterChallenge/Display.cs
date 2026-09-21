using System;
using System.Collections.Generic;
using System.Drawing;
using System.Xml.Linq;
using static System.Formats.Asn1.AsnWriter;

namespace WinterChallenge2024;
internal static class Display
{
    internal static void ProteinStock(ProteinStock proteinStock)
    {
        Logger.Line($"A: {proteinStock.A}");
        Logger.Line($"B: {proteinStock.B}");
        Logger.Line($"C: {proteinStock.C}");
        Logger.Line($"D: {proteinStock.D}");
    }

    internal static void Proteins(List<Protein> proteins)
    {
        Logger.Line($"Proteins");

        proteins.ForEach(p =>
            Logger.Line($"Type:{p.Type} - Position:({p.Position.X},{p.Position.Y}) - BeingHarvested:{p.IsHarvested}"));
    }

    internal static void Organisms(List<Organism> organisms)
    {
        foreach (Organism organism in organisms)
        {
            Organism(organism);
            Logger.Line("-----------------------------------");
        }
    }

    internal static void Organism(Organism organism)
    {
        foreach (Organ organ in organism.Organs)
        {
            switch (organ.Type)
            {
                case OrganType.BASIC:
                case OrganType.ROOT:
                    Logger.Line($" ID:{organ.Id} - Type:{organ.Type.ToString()} - Position:({organ.Position.X},{organ.Position.Y})");
                    break;

                case OrganType.HARVESTER:
                case OrganType.SPORER:
                case OrganType.TENTACLE:
                    Logger.Line($" ID:{organ.Id} - Type:{organ.Type.ToString()} - Position:({organ.Position.X},{organ.Position.Y}) - Direction:{organ.Direction.ToString()}");
                    break;
            }
        }
    }

    internal static void Nodes(List<Node> nodes)
    {
        nodes.ForEach(n =>
            Logger.Line($"Position:({n.Position.X},{n.Position.Y}) - Closed:{n.Closed}"));
    }

    internal static void Map(Game game)
    {
        string[,] map = new string[game.Width, game.Height];

        for (int y = 0; y < game.Height; y++)
        {
            for (int x = 0; x < game.Width; x++)
            {
                map[x, y] = " ";
            }
        }

        for (int y = 0; y < game.Height; y++)
        {
            for (int x = 0; x < game.Width; x++)
            {
                if (game.Walls[x, y])
                {
                    map[x, y] = "X";
                }
            }
        }

        foreach (Protein protein in game.Proteins)
        {
            map[protein.Position.X, protein.Position.Y] = protein.Type.ToString();
        }

        foreach (Organism organism in game.PlayerOrganisms)
        {
            foreach (Organ organ in organism.Organs)
            {
                map[organ.Position.X, organ.Position.Y] = "O";
            }
        }

        foreach (Organism organism in game.OpponentOrganisms)
        {
            foreach (Organ organ in organism.Organs)
            {
                map[organ.Position.X, organ.Position.Y] = "o";
            }
        }

        Logger.Line("----------");
        for (int y = 0; y < game.Height; y++)
        {
            string row = "|";

            for (int x = 0; x < game.Width; x++)
            {
                row += map[x, y];
            }

            row += "|";

            Logger.Line(row);
        }
        Logger.Line("----------");
    }

    internal static void TimeStamp(long totalTime, long segmentTime, string task)
    {
        TimeSpan total = TimeSpan.FromTicks(totalTime);
        TimeSpan segment = TimeSpan.FromTicks(segmentTime);
        Logger.Line($"{total.Milliseconds}ms-{segment.Milliseconds}ms-{task}");
    }

    internal static void Actions(List<Action> actions)
    {
        foreach (Action action in actions)
        {
            // Logger.Line($"Goal type:{action.GoalType}, Protein type:{action.GoalProteinType}, Turns:{action.TurnsToGoal}, score:{action.Score}");
            Logger.Line(action.ToString() + $" - score:{ action.Score} - from {action.Source}");
        }
    }

    internal static void ActionsDictionary(Dictionary<int, List<Action>> actionsDictionarly)
    {
        foreach (KeyValuePair<int, List<Action>> actions in actionsDictionarly)
        {
            Logger.Line("-----------------------------------");
            Logger.Line($"OrganismId:{actions.Key}");
            Actions(actions.Value);
        }
    }

    internal static void ActionSources(Dictionary<ActionSource, int> trackedActions)
    {
        Logger.Line("Tracked actions count");
        foreach (KeyValuePair<ActionSource, int> trackedAction in trackedActions)
        {
            Logger.Line($"{trackedAction.Key} - {trackedAction.Value}");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Xml.Linq;

namespace WinterChallenge2024;
internal static class Display
{
    internal static void Summary(Game game)
    {
        Console.Error.WriteLine($"PROTEINS");
        Proteins(game.Proteins);
        Console.Error.WriteLine("==================================");

        Console.Error.WriteLine($"ORGANISMS");
        Console.Error.WriteLine("----------------------------------");
        Console.Error.WriteLine($"Player organisms");
        Organisms(game.PlayerOrganisms);
        Console.Error.WriteLine("----------------------------------");
        Console.Error.WriteLine($"Opponent organisms");
        Organisms(game.OpponentOrganisms);
        Console.Error.WriteLine("==================================");

        Console.Error.WriteLine($"PROTEIN STOCK");
        Console.Error.WriteLine("----------------------------------");
        Console.Error.WriteLine($"Player protein stock");
        ProteinStock(game.PlayerProteinStock);
        Console.Error.WriteLine("----------------------------------");
        Console.Error.WriteLine($"Opponent protein stock");
        ProteinStock(game.OpponentProteinStock);
        Console.Error.WriteLine("==================================");
    }

    internal static void ProteinStock(ProteinStock proteinStock)
    {
        Console.Error.WriteLine($"A: {proteinStock.A}");
        Console.Error.WriteLine($"B: {proteinStock.B}");
        Console.Error.WriteLine($"C: {proteinStock.C}");
        Console.Error.WriteLine($"D: {proteinStock.D}");
    }

    internal static void Proteins(List<Protein> proteins)
    {
        Console.Error.WriteLine($"Proteins");

        proteins.ForEach(p => 
            Console.Error.WriteLine($"Type:{p.Type} - Position:({p.Position.X},{p.Position.Y}) - BeingHarvested:{p.IsHarvested}"));
    }

    internal static void Organisms(List<Organism> organisms)
    {
        foreach (Organism organism in organisms)
        {
            Organism(organism);
            Console.Error.WriteLine("-----------------------------------");
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
                    Console.Error.WriteLine($" ID:{organ.Id} - Type:{organ.Type.ToString()} - Position:({organ.Position.X},{organ.Position.Y})");
                    break;

                case OrganType.HARVESTER:
                case OrganType.SPORER:
                    Console.Error.WriteLine($" ID:{organ.Id} - Type:{organ.Type.ToString()} - Position:({organ.Position.X},{organ.Position.Y}) - Direction:{organ.Direction.ToString()}");
                    break;
            }
        }
    }

    internal static void Nodes(List<Node> nodes)
    {
        nodes.ForEach(n => 
            Console.Error.WriteLine($"Position:({n.Position.X},{n.Position.Y}) - Closed:{n.Closed}"));
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

        foreach (Point wall in game.Walls)
        {
            map[wall.X, wall.Y] = "X";
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

        for (int y = 0; y < game.Height; y++)
        {
            string row = string.Empty;

            for (int x = 0; x < game.Width; x++)
            {
                row += map[x, y];
            }

            Console.Error.WriteLine(row);
        }
    }
}

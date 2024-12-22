using System;
using System.Collections.Generic;

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

        foreach (Protein protein in proteins)
        {
            Console.Error.WriteLine($"Type:{protein.Type} - Position:({protein.Position.X},{protein.Position.Y}) - BeingHarvested:{protein.IsHarvested}");
        }
    }

    internal static void Organisms(List<Organism> organisms)
    {
        foreach (Organism organism in organisms)
        {
            Display.Organism(organism);
            Console.Error.WriteLine("-----------------------------------");
        }
    }

    internal static void Organism(Organism organism)
    {
        foreach (Organ organ in organism.Organs)
        {
            if (organ.Type == OrganType.ROOT)
            {
                Console.Error.WriteLine($" ID:{organ.Id} - Type:ROOT - Position:({organ.Position.X},{organ.Position.Y})");
            }
            if (organ.Type == OrganType.BASIC)
            {
                Console.Error.WriteLine($" ID:{organ.Id} - Type:BASIC - Position:({organ.Position.X},{organ.Position.Y})");
            }
            else if (organ.Type == OrganType.HARVESTER)
            {
                Console.Error.WriteLine($" ID:{organ.Id} - Type:HARVESTER - Position:({organ.Position.X},{organ.Position.Y}) - Direction:{organ.Direction.ToString()}");
            }
        }
    }

    internal static void Nodes(List<Node> nodes)
    {
        foreach(Node node in nodes)
        {
            Console.Error.WriteLine($"Position:({node.Position.X},{node.Position.Y}) - Closed:{node.Closed}");
        }
    }
}

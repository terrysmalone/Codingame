using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterChallenge2024;
internal static class Display
{
    internal static void Summary(Game game)
    {
        Console.Error.WriteLine($"Proteins");
        Proteins(game.Proteins);
        Console.Error.WriteLine("==================================");

        Console.Error.WriteLine($"Protein stock");
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
            Console.Error.WriteLine($"Type:{protein.Type} - Position:({protein.Position.X},{protein.Position.Y})");
        }
    }
}

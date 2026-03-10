namespace CodeRoyale;

using System;
using System.Collections.Generic;

internal static class Debug
{
    internal static void Sites(List<Site> sites)
    {
        Console.Error.WriteLine("Sites");
        foreach (Site site in sites) 
        {
            Console.Error.WriteLine($"Site {site.Id} at {site.Position.X},{site.Position.Y} owned by {site.Owner} with structure {site.Structure}");

            if (site.Structure == StructureType.Mine)
            {
                Console.Error.WriteLine($"  Mine size: {site.MineSize}/{site.MaxMineSize} with gold {site.Gold}");
            }
        }
    }

    internal static void Units(string debugMessage, List<Unit> units)
    {
        Console.Error.WriteLine(debugMessage);
        Console.Error.WriteLine("------");

        foreach (Unit unit in units)
        {
            Console.Error.WriteLine($"Type:{unit.Type}");
            Console.Error.WriteLine($"Position:{unit.Position.X},{unit.Position.Y}");
            Console.Error.WriteLine($"Position:{unit.Health}");
            Console.Error.WriteLine("------");
        }

        Console.Error.WriteLine();
    }
}

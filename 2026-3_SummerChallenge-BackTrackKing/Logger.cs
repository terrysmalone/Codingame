using System;
using System.Collections.Generic;

namespace BackTrackKing;

internal static class Logger
{

    private static bool DISABLE_LOGGING = false;

    internal static void DisableLogging()
    {
        DISABLE_LOGGING = true;
    }

    internal static void EnableLogging()
    {
        DISABLE_LOGGING = false;
    }

    internal static void Error(string message)
    {
        Console.Error.WriteLine("ERROR: " + message);
    }

    internal static void Message(string message)
    {
        if (DISABLE_LOGGING) 
        {
            return;
        }

        Console.Error.WriteLine(message);
    }

    internal static void DesirePaths(List<DesirePath> desirePaths)
    {
        if (DISABLE_LOGGING)
        {
            return;
        }

        Console.Error.WriteLine($"DesirePaths");
        foreach (var desirePath in desirePaths)
        {
            Console.Error.WriteLine($"{desirePath.FullPath[0].X},{desirePath.FullPath[0].Y} -> {desirePath.FullPath[desirePath.FullPathCount-1].X},{desirePath.FullPath[desirePath.FullPathCount - 1].Y} - Path:{desirePath.RemainingPathCount}/{desirePath.FullPathCount}, Action:{desirePath.RemainingActionCount}/{desirePath.FullActionCount}");
        }
    }

    internal static void TypeMap(Map map)
    {
        if (DISABLE_LOGGING)
        {
            return;
        }

        for (int y = 0; y < map.Height; y++)
        {
            for (int x = 0; x < map.Width; x++)
            {
                Console.Error.Write(ToSymbol(map.CellTypes[x, y]));
            }
            Console.Error.WriteLine();
        }
    }

    private static string ToSymbol(CellType cellType)
    {
        switch (cellType)
        {
                case CellType.PLAINS:
                    return " ";
                case CellType.RIVER:
                    return "~";
                case CellType.MOUNTAIN:
                    return "^";
                case CellType.POI:
                    return "*";
                default:
                    return " ";
        }
    }

    internal static void Regions(List<Region> regions)
    {
        if (DISABLE_LOGGING)
        {
            return;
        }

        Console.Error.WriteLine("REGIONS");

        foreach (var region in regions)
        {
            Console.Error.WriteLine($"{region.Id}: {region.GetActiveConnectionScore()}");
        }
    }
}
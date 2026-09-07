using System;

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

    internal static void Message(string message)
    {
        if (DISABLE_LOGGING) 
        {
            return;
        }

        Console.Error.WriteLine(message);
    }

    internal static void RegionMap(Map map)
    {      

    }

    internal static void TypeMap(Map map)
    {
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
}
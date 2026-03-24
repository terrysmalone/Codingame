using System;
using System.Drawing;

namespace TronBattle;

internal static class Logger
{
    internal static void LightCyclePosition(Point position0, Point position1)
    {
        Console.Error.WriteLine($"Player position: ({position0.X},{position0.Y}) - ({position1.X},{position1.Y}) ");
    }

    internal static void LightCyclePositions(Point[] positions0, Point[] positions1)
    {
        for (int i = 0; i < positions0.Length; i++)
        {
            Console.Error.WriteLine($"Enemy position: ({positions0[i].X},{positions0[i].Y}) - ({positions1[i].X},{positions1[i].Y}) ");
        }

    }
}

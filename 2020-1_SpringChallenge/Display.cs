namespace SpringChallenge2020;

using System;
using System.Collections.Generic;

internal static class Display
{
    internal static void PelletDistances(List<PelletDistance> pelletDistances)
    {
        foreach (PelletDistance pelletDistance in pelletDistances)
        {
            Console.Error.WriteLine($"Pellet ({pelletDistance.Position.X}, {pelletDistance.Position.Y}): [{string.Join(" ", pelletDistance.Distances)}]");
        }
    }
}
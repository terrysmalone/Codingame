namespace Fall2024Challenge_SeleniaCity;

using System;
using System.Collections.Generic;

internal static class Display 
{
    internal static void Tubes(List<Tube> tubes)
    {
        if (tubes.Count == 0)
            return;

        Console.Error.WriteLine("");
        foreach (Tube tube in tubes)
        { 
            Console.Error.WriteLine($"{tube.Building1Id} -> {tube.Building2Id} Capacity:{tube.Capacity}");
        }
        Console.Error.WriteLine("");
    }

    internal static void Pods(List<Pod> pods)
    {
        if (pods.Count == 0)
            return;

        Console.Error.WriteLine("");
        foreach (Pod pod in pods)
        {
            Console.Error.WriteLine($"{pod.Id}- Stops:{pod.NumberOfStops} - {string.Join(" ", pod.Path)}");
        }
        Console.Error.WriteLine("");
    }
}

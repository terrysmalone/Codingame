namespace Fall2024Challenge_SeleniaCity;

using System;
using System.Collections.Generic;

internal static class Display 
{
    internal static void Summary(Game game, bool verbose)
    {
        Console.Error.WriteLine("==================================");
        Console.Error.WriteLine($"Resources: {game.Resources}");
        Console.Error.WriteLine();

        // Landing Pads
        Console.Error.WriteLine($"Landing Pads: {game.LandingPads.Count}");
        if (verbose)
        {
            LandingPads(game.LandingPads);
        }
        Console.Error.WriteLine();

        // Modules
        Console.Error.WriteLine($"Modules: {game.Modules.Count}");
        if (verbose)
        {
            Modules(game.Modules);
        }
        Console.Error.WriteLine();

        // Tubes
        Console.Error.WriteLine($"Tubes: {game.Tubes.Count}");
        if (verbose)
        {
            Tubes(game.Tubes);
        }
        Console.Error.WriteLine();

        // Pods
        Console.Error.WriteLine($"Pods: {game.Pods.Count}");
        if (verbose)
        {
            Pods(game.Pods);
        }
        Console.Error.WriteLine();

        Console.Error.WriteLine("==================================");
    }

    internal static void LandingPads(List<LandingPad> landingPads)
    {
        if (landingPads.Count == 0)
            return;

        Console.Error.WriteLine("");
        foreach (LandingPad landingPad in landingPads)
        {
            Console.Error.WriteLine($"{landingPad.Id}: ({landingPad.Position}) - Astronauts: {landingPad.Astronauts}");
        }
        Console.Error.WriteLine("");
    }

    internal static void Modules(List<Module> modules)
    {
        if (modules.Count == 0)
            return;

        Console.Error.WriteLine("");
        foreach (Module module in modules)
        {
            Console.Error.WriteLine($"{module.Id}: ({module.Position}) - Type: {module.Type}");
        }
        Console.Error.WriteLine("");
    }

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

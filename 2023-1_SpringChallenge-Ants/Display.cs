using System;
using System.Collections.Generic;
using System.IO;

namespace _2023_1_SpringChallenge_Ants;

internal static class Display
{
    internal static void Path(string message, List<int> path)
    {
        if (path.Count == 0)
        {
            return;
        }

        Console.Error.WriteLine(message);
        Console.Error.WriteLine($"Path to {path[path.Count - 1]}: {string.Join("->", path)}");
    }

    internal static void Paths(string message, List<List<int>> paths)
    {
        Console.Error.WriteLine(message);
        foreach (var path in paths)
        {
            if (path.Count == 0)
            {
                continue;
            }

            Console.Error.WriteLine($"Path to {path[path.Count-1]}: {string.Join("->", path)}");
        }
    }

    internal static void ResourcePaths(string message, List<ResourcePath> resourcePaths)
    {
        Console.Error.WriteLine(message);

        foreach (var resourcePath in resourcePaths)
        {
            Console.Error.WriteLine($"Path ({resourcePath.CellType}) - Id:{resourcePath.PathId} ParentId:{resourcePath.ParentPathId} - {string.Join("->", resourcePath.Path)}");
        }


    }
}



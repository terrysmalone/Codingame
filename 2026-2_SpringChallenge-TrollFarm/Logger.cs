using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

namespace SpringChallenge2026;

internal static class Logger    
{
    private static bool DISABLE_LOGGING = false;
    private static bool DISABLE_TIMES = false;

    private static long _roundStartTime;
    private static long _lastTimedLog;

    internal static void Message(string message)
    {
        if (DISABLE_LOGGING)
        {
            return;
        }

        Console.Error.WriteLine(message);
    }

    internal static void DisableLogging() 
    {
        DISABLE_LOGGING = true;
    }

    internal static void EnableLogging()
    {
        DISABLE_LOGGING = false;
    }

    internal static void StartRoundStopwatch()
    {
        if (DISABLE_TIMES)
        {
            return;
        }
        _roundStartTime = Stopwatch.GetTimestamp();
        _lastTimedLog = Stopwatch.GetTimestamp();
    }

    internal static void LogTime(string message)
    {
        if (DISABLE_TIMES)
        {
            return;
        }
        TimeSpan elapsedTime = Stopwatch.GetElapsedTime(_roundStartTime);
        TimeSpan elapsedSinceLastLog = Stopwatch.GetElapsedTime(_lastTimedLog);
        Console.Error.WriteLine($"{elapsedTime.TotalMilliseconds}({elapsedSinceLastLog.TotalMilliseconds}): {message}");
        _lastTimedLog = Stopwatch.GetTimestamp();
    }

    internal static void Inventory(string message,Inventory inventory)
    {
        if (DISABLE_LOGGING)
        {
            return;
        }

        Console.Error.WriteLine(message);
        Console.Error.WriteLine($"Plum: {inventory.Plum}, Lemon: {inventory.Lemon}, Apple: {inventory.Apple}, Banana: {inventory.Banana}, Iron: {inventory.Iron}, Wood: {inventory.Wood}");
    }

    internal static void Path(string message, List<Point> path)
    {
        if (DISABLE_LOGGING)
        {
            return;
        }

        Console.Error.WriteLine(message);
        Console.Error.WriteLine($"Path: {string.Join("->", path)}");

    }

    internal static void Troll(Troll troll)
    {
        if (DISABLE_LOGGING)
        {
            return;
        }

        Console.Error.WriteLine($"Troll {troll.Id} at {troll.Position}, MovementSpeed: {troll.MovementSpeed}, CarryCapacity: {troll.CarryCapacity}, HarvestPower: {troll.HarvestPower}, ChopPower: {troll.ChopPower}, CarryPlum: {troll.CarryPlum}, CarryLemon: {troll.CarryLemon}, CarryApple: {troll.CarryApple}, CarryBanana: {troll.CarryBanana}, CarryIron: {troll.CarryIron}, CarryWood: {troll.CarryWood}");
    }

    internal static void Prioirities(List<Need> priorities)
    {
        if (DISABLE_LOGGING)
        {
            return;
        }

        Console.Error.WriteLine("PRIORITIES:");
        foreach (Need need in priorities)
        {
            Console.Error.WriteLine(need);
        }
    }

    internal static void Error(string message)
    {
        Console.Error.WriteLine($"ERROR: {message}");
    }

    internal static void Trolls(List<Troll> playerTrolls)
    {
        if (DISABLE_LOGGING)
        {
            return;
        }

        Console.Error.WriteLine("TROLLS:");
        foreach (Troll troll in playerTrolls)
        {
            Troll(troll);
        }
    }

    internal static void Trees(List<Tree> trees)
    {
        if (DISABLE_LOGGING)
        {
            return;
        }

        foreach (Tree tree in trees)
        {
            Console.Error.WriteLine($"Tree at {tree.Position}, Type: {tree.Type}, Size: {tree.Size}, Health: {tree.Health}, Fruits: {tree.Fruits}, Cooldown: {tree.Cooldown}");
        }

    }        
}
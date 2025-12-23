using System;
using System.Collections.Generic;
using System.Linq;

namespace _2023_2_FallChallenge_SeabedSecurity;

internal static class Logger
{
    internal static void AllDrones(string? message, List<Drone> drones)
    {
        if (message != null)
        {
            Console.Error.WriteLine(message);
            Console.Error.WriteLine("==================================");
        }

        foreach (Drone drone in drones)
        {
            Drone(drone);
            Console.Error.WriteLine("------------------------");
        }
    }

    internal static void Drone(Drone drone)
    {
        Console.Error.WriteLine($"Drone {drone.Id}");
        Console.Error.WriteLine($"Position: {drone.Position.X},{drone.Position.Y}");
        Console.Error.WriteLine($"Battery:  {drone.BatteryLevel}");

        Console.Error.WriteLine($"Stored scans: {string.Join(" ", drone.ScannedCreaturesIds)}");

        Console.Error.WriteLine($"Creature directions: {string.Join(" ", drone.CreatureDirections.Select(kv => $"{kv.Key}:{kv.Value}"))}");
    }
}
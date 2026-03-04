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

    internal static void AllMonsters(List<Creature> creatures)
    {
        Console.Error.WriteLine("==================================");
        Console.Error.WriteLine("All Monsters");
        Console.Error.WriteLine("----------------------------------");

        foreach (Creature creature in creatures)
        {
            if (creature.Type != -1)
            {
                continue;
            }
            
            Creature(creature);
            Console.Error.WriteLine("------------------------");
        }
    }


    internal static void Creature(Creature creature)
    {
        Console.Error.WriteLine($"Creature {creature.Id}");
        Console.Error.WriteLine($"Visible: {creature.IsVisible}");
        Console.Error.WriteLine($"Last seen round: {creature.LastSeenRound}");
        Console.Error.WriteLine($"Type: {creature.Type}");
        Console.Error.WriteLine($"Color: {creature.Color}");
        Console.Error.WriteLine($"Position: {creature.Position.X},{creature.Position.Y}");
        Console.Error.WriteLine($"Velocity: {creature.Velocity.X},{creature.Velocity.Y}");
    }
}
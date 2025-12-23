namespace _2023_2_FallChallenge_SeabedSecurity;

using System;
using System.Collections.Generic;
using System.Drawing;

internal sealed class Drone
{
    internal int Id { get; private set; }

    internal Point Position { get; set; }
    internal int BatteryLevel { get; set; }

    internal HashSet<int> ScannedCreaturesIds { get; private set; } = new();

    internal Dictionary<int, CreatureDirection> CreatureDirections = new Dictionary<int, CreatureDirection>();

    internal Drone(int id, int xPos, int yPos, int batteryLevel)
    {
        Id = id;
        Position = new Point(xPos, yPos);
        BatteryLevel = batteryLevel;
    }

    internal void AddScannedCreatures(int id)
    {
        ScannedCreaturesIds.Add(id);
    }

    internal void AddCreatureDirection(int creatureId, CreatureDirection direction)
    {
        CreatureDirections[creatureId] = direction;
    }
}
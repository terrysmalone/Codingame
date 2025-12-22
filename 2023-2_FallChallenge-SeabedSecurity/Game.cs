namespace _2023_2_FallChallenge_SeabedSecurity;

using System;
using System.Collections.Generic;
using System.Drawing;

internal class Game
{
    internal int MyScore { get; set; }
    internal int EnemyScore { get; set; }

    private List<Creature> creatures = [];
    private List<Drone> myDrones = [];
    private List<Drone> enemyDrones = [];

    private DistanceCalculator distanceCalculator;

    internal void InitialiseCreatures(List<Creature> allCreatures)
    {
        creatures = allCreatures;
        distanceCalculator = new DistanceCalculator(creatures);
    }

    internal void AddScannedCreature(int creatureId, bool isMyDrone)
    {
        Creature creature = creatures.Find(c => c.Id == creatureId);
        if (creature != null)
        {
            if (isMyDrone)
            {
                creature.IsScannedByMe = true;
            }
            else
            {
                creature.IsScannedByEnemy = true;
            }
        }
    }

    internal void SetMyDrones(List<Drone> drones)
    {
        myDrones = drones;
    }

    internal void SetEnemyDrones(List<Drone> drones)
    {
        enemyDrones = drones;
    }

    internal void UpdateCreaturePosition(int creatureId, int creatureX, int creatureY, int creatureVx, int creatureVy)
    {
        Creature creature = creatures.Find(c => c.Id == creatureId);
        if (creature != null)
        {
            creature.Position = new Point(creatureX, creatureY);
            creature.Velocity = new Point(creatureVx, creatureVy);
        }
    }

    internal List<string> CalculateActions()
    {
        // First simple pass solution
        // For each drone, move towards the nearest unscanned creature

        var actions = new List<string>();

        for (var i=0; i<myDrones.Count; i++)
        {
            Point pos = distanceCalculator.GetClosestCreaturePosition(myDrones[i], true, true);
            actions.Add($"MOVE {pos.X} {pos.Y} 1 Battery level:{myDrones[i].BatteryLevel}"); // MOVE <x> <y> <light (1|0)> | WAIT <light (1|0)>
        }

        return actions;
    }
}



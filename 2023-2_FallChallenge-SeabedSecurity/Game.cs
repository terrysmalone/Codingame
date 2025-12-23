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

    internal HashSet<int> MyScannedCreatureIds { get; private set; } = new();
    internal HashSet<int> EnemyScannedCreatureIds { get; private set; } = new();

    private DistanceCalculator distanceCalculator;
    private DirectionCalculator directionCalculator;

    internal int visibleCreatureCount;

    internal void InitialiseCreatures(List<Creature> allCreatures)
    {
        creatures = allCreatures;
        distanceCalculator = new DistanceCalculator(creatures);
        directionCalculator = new DirectionCalculator(this);
    }

    internal void AddScannedCreature(int creatureId, bool isMyDrone)
    {      
        if (isMyDrone)
        {
            MyScannedCreatureIds.Add(creatureId);
        }
        else
        {
            EnemyScannedCreatureIds.Add(creatureId);
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
            creature.IsVisible = true;

            visibleCreatureCount++;
        }
    }

    internal void SetAllCreaturesAsNotVisible()
    {
        visibleCreatureCount = 0;

        foreach (var creature in creatures)
        {
            creature.IsVisible = false;
        }
    }

    internal List<string> CalculateActions()
    {
        var actions = new List<string>();

        for (var i = 0; i < myDrones.Count; i++)
        {
            var drone = myDrones[i];
            Logger.Drone(drone);

            // If stored scans >= 4 head to surface
            if (drone.ScannedCreaturesIds.Count >= 4 || MyScannedCreatureIds.Count + drone.ScannedCreaturesIds.Count >= 12)
            {
                actions.Add($"MOVE {drone.Position.X} 500 0");
            }
            else
            {
                var lightLevel = 0;

                if(visibleCreatureCount > 0)
                {
                    lightLevel = 1;
                }


                // Move in the direction of the most unscanned fish
                CreatureDirection direction = directionCalculator.GetBestDirectionFromRadarBlips(drone);

                var targetPosition = new Point(0, 0);

                switch (direction)
                {
                    case CreatureDirection.TL:
                        targetPosition = new Point(drone.Position.X - 1000, drone.Position.Y - 1000);
                        break;
                    case CreatureDirection.TR:
                        targetPosition = new Point(drone.Position.X + 1000, drone.Position.Y - 1000);
                        break;
                    case CreatureDirection.BL:
                        targetPosition = new Point(drone.Position.X - 1000, drone.Position.Y + 1000);
                        break;
                    case CreatureDirection.BR:
                        targetPosition = new Point(drone.Position.X + 1000, drone.Position.Y + 1000);
                        break;
                }

                actions.Add($"MOVE {targetPosition.X} {targetPosition.Y} {lightLevel} {drone.BatteryLevel}");
            }
        }

        return actions;
    }
}



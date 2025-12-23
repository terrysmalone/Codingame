namespace _2023_2_FallChallenge_SeabedSecurity;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

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
    internal int round = 0;

    internal bool earlyGame = true;

    internal Dictionary<int, int> lastRoundTorchUsed = new();

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
        Console.Error.WriteLine($"Visible creatures this turn: {visibleCreatureCount}");

        round++;

        var actions = new List<string>();

        if (earlyGame)
        {
            // in the early game drop as far as possible, picking up fish thenrise to the top
            for (var i = 0; i < myDrones.Count; i++)
            {
                var drone = myDrones[i];

                var lightLevel = CalculateLightLevel(drone);

                // Drop to 8,000+ then rise to the top
                if (drone.Position.Y >= 9000)
                {
                    earlyGame = false;
                    
                }

                actions.Add($"MOVE {drone.Position.X} 9500 {lightLevel}");
            }
        }
        else
        {
            // in the mid game be more deliberate about where to go
            for (var i = 0; i < myDrones.Count; i++)
            {
                var drone = myDrones[i];
                var lightLevel = CalculateLightLevel(drone);

                // If stored scans >= 4 head to surface
                if (drone.ScannedCreaturesIds.Count >= 4 || MyScannedCreatureIds.Count + drone.ScannedCreaturesIds.Count >= 12)
                {
                    actions.Add($"MOVE {drone.Position.X} 500 {lightLevel}");
                }
                else
                {
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
        }

        return actions;
    }

    private int CalculateLightLevel(Drone drone)
    {
        var lightLevel = 0;

        if (lastRoundTorchUsed.ContainsKey(drone.Id))
        {
            var lastUsed = lastRoundTorchUsed[drone.Id];

            if (round - lastUsed >= 5)
            {
                lightLevel = 1;
                lastRoundTorchUsed[drone.Id] = round;
            }
        }
        else
        {
            lastRoundTorchUsed[drone.Id] = 0;
        }

        return lightLevel;
    }
}



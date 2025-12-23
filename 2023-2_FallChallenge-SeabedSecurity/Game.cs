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

    private DirectionCalculator directionCalculator;

    internal int visibleCreatureCount;
    internal HashSet<int> VisibleMonsterIds { get; set; } = new();

    internal int round = 0;

    internal bool earlyGame = true;

    internal Dictionary<int, int> lastRoundTorchUsed = new();

    internal void InitialiseCreatures(List<Creature> allCreatures)
    {
        creatures = allCreatures;
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

            if (creature.Type != -1)
            {
                visibleCreatureCount++;
            }
            else
            {
                VisibleMonsterIds.Add(creatureId);
            }
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

        foreach (var drone in myDrones)
        {
            var action = string.Empty;

            var lightLevel = CalculateLightLevel(drone);

            Console.Error.WriteLine($"Drone {drone.Id}");

            // If the drone is within 2000 of a creature of type -1 set early game to false and evade the creature
            if (VisibleMonsterIds.Count > 0)
            {
                Console.Error.WriteLine($"Checking for monsters to evade...");
                foreach (var monsterId in VisibleMonsterIds)
                {
                    var monster = creatures.Find(c => c.Id == monsterId);
                    var distanceToMonster = DistanceCalculator.GetDistance(drone.Position, monster.Position);
                    Console.Error.WriteLine($"Distance to monster {monster.Id}: {distanceToMonster}");
                    if (distanceToMonster <= 2000)
                    {
                        Console.Error.WriteLine($"Evading monster {monster.Id}");
                        earlyGame = false;
                        // Move away from the monster
                        var directionX = drone.Position.X - monster.Position.X;
                        var directionY = drone.Position.Y - monster.Position.Y;

                        Console.Error.WriteLine($"Direction before normalization: ({directionX}, {directionY})");

                        //var length = (int)Math.Sqrt(directionX * directionX + directionY * directionY);
                        //directionX /= length;
                        //directionY /= length;

                        Console.Error.WriteLine($"Direction after normalization: ({directionX}, {directionY})");

                        var targetX = drone.Position.X + directionX;
                        var targetY = drone.Position.Y + directionY;
                        Console.Error.WriteLine($"Evade target position: ({targetX}, {targetY})");

                        action = $"MOVE {targetX} {targetY} {lightLevel}";
                        break;
                    }
                }
            }

            if (action != string.Empty)
            {
                actions.Add(action);
                continue;
            }

            if (earlyGame)
            {
                // Drop to 8,000+ then rise to the top
                if (drone.Position.Y >= 9000)
                {
                    earlyGame = false;

                }

                actions.Add($"MOVE {drone.Position.X} 9500 {lightLevel}");
            }
            else
            {
                // If stored scans >= 4 head to surface
                if (drone.ScannedCreaturesIds.Count >= 3 || MyScannedCreatureIds.Count + drone.ScannedCreaturesIds.Count >= 12)
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



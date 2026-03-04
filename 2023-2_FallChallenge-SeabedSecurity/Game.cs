namespace _2023_2_FallChallenge_SeabedSecurity;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Threading;

internal class Game
{
    private const int _droneSpeed = 600;
    private const int _monsterDashSpeed = 540;
    private const int _captureSize = 500;

    internal int MyScore { get; set; }
    internal int EnemyScore { get; set; }

    private List<Creature> creatures = [];
    private List<Drone> myDrones = [];
    private List<Drone> enemyDrones = [];

    internal HashSet<int> MyStoredCreatureIds { get; private set; } = new();
    internal HashSet<int> EnemyStoredCreatureIds { get; private set; } = new();

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

    internal void AddStoredCreature(int creatureId, bool isMyDrone)
    {      
        if (isMyDrone)
        {
            MyStoredCreatureIds.Add(creatureId);
        }
        else
        {
            EnemyStoredCreatureIds.Add(creatureId);
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
            creature.LastSeenRound = round;

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
        Console.Error.WriteLine($"Round {round} calculate actions");

        Logger.AllMonsters(creatures);

        var actions = new List<string>();

        // If a union of stored creatures and currently scanned creatures by both drones is equal to 12, we know all the creatures and can just head to the surface with all of them
        var allKnownCreatures = new HashSet<int>(MyStoredCreatureIds);
        allKnownCreatures.UnionWith(myDrones[0].ScannedCreaturesIds);
        allKnownCreatures.UnionWith(myDrones[1].ScannedCreaturesIds);

        if (allKnownCreatures.Count >= 12)
        {
            earlyGame = false;
        }

        foreach (var drone in myDrones)
        {
            var action = string.Empty;

            var lightLevel = CalculateLightLevel(drone);

            Console.Error.WriteLine($"Drone {drone.Id}");

            HashSet<int> monstersToAvoid = new HashSet<int>();

            // First add visible monsters to the avoid list
            if (VisibleMonsterIds.Count > 0)
            {
                foreach (var monsterId in VisibleMonsterIds)
                {
                    var monster = creatures.Find(c => c.Id == monsterId);

                    var distanceToMonster = DistanceCalculator.GetDistance(drone.Position, monster.Position);

                    if (distanceToMonster <= _droneSpeed + _monsterDashSpeed + _captureSize)
                    {
                        Console.Error.WriteLine($"Adding visible monsters to avoid: {monsterId}");
                        monstersToAvoid.Add(monsterId);
                    }
                }
            }

            // Now add close monsters seen last round to the avoid list
            foreach (var creature in creatures)
            {
                if (creature.Type != -1 || creature.IsVisible)
                {
                    continue;
                }

                var distanceToMonster = DistanceCalculator.GetDistance(drone.Position, creature.Position);

                Console.Error.WriteLine($"Checking non visible monster {creature.Id} at distance {distanceToMonster} last seen {round - creature.LastSeenRound} rounds ago");

                if (round - creature.LastSeenRound == 1)
                {
                    Console.Error.WriteLine($"Monster {creature.Id} was seen last round, distance to monster: {distanceToMonster}: distance cutoff: {_droneSpeed + (2 * _monsterDashSpeed) + _captureSize}");
                }
                if (round - creature.LastSeenRound == 2)
                {
                    Console.Error.WriteLine($"Monster {creature.Id} was seen 2 rounds ago, distance to monster: {distanceToMonster}: distance cutoff: {_droneSpeed + (3 * _monsterDashSpeed) + _captureSize}");
                }

                // TODO: Increase all by _monsterDashSpeed
                if ((round - creature.LastSeenRound == 1 && distanceToMonster <= _droneSpeed + (2 *_monsterDashSpeed) + _captureSize) ||
                    (round - creature.LastSeenRound == 2 && distanceToMonster <= _droneSpeed + (3 * _monsterDashSpeed) + _captureSize))
                {
                    Console.Error.WriteLine($"Adding non visible monster to avoid: {creature.Id}");
                    monstersToAvoid.Add(creature.Id);
                }
            }

            Console.Error.WriteLine($"Monsters to avoid: {string.Join(", ", monstersToAvoid)}");

            if (monstersToAvoid.Count > 0)
            {
                Point avoidancePosition = CalculateAvoidanceVector(drone.Position, monstersToAvoid);
                action = $"MOVE {avoidancePosition.X} {avoidancePosition.Y} {lightLevel} RUNNING AWAY";
            }

            if (action != string.Empty)
            {
                actions.Add(action);
                continue;
            }

            if (earlyGame)
            {
                // Drop to 8,000+ then rise to the top
                if (drone.Position.Y >= 8000)
                {
                    earlyGame = false;
                }

                // Move to the centre of it's side
                int xPos = 0;

                if(drone.Position.X < 5000)
                {
                    xPos = 2500;
                }
                else
                {
                    xPos = 7500;
                }
                
                actions.Add($"MOVE {xPos} 9500 {lightLevel} EARLY GAME DIVING");
            }
            else
            {
                if (drone.ScannedCreaturesIds.Count >= 3 || allKnownCreatures.Count >= 12)
                {
                    actions.Add($"MOVE {drone.Position.X} 500 {lightLevel} HEADING TO SURFACE");
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

                    actions.Add($"MOVE {targetPosition.X} {targetPosition.Y} {lightLevel}  HEADING TO UNSCANNED FISH");
                }
            }
        }


        round++;

        return actions;
    }

    private Point CalculateAvoidanceVector(Point dronePosition, HashSet<int> monstersToAvoid)
    {
        double totalForceX = 0;
        double totalForceY = 0;

        foreach (var monsterId in monstersToAvoid)
        {
            var monster = creatures.Find(c => c.Id == monsterId);
            var dx = dronePosition.X - monster.Position.X;
            var dy = dronePosition.Y - monster.Position.Y;
            var distance = Math.Sqrt(dx * dx + dy * dy);

            if (distance > 0)
            {
                // Normalize direction - each monster contributes equally
                totalForceX += dx / distance;
                totalForceY += dy / distance;
            }
        }

        // Normalize the combined vector and scale to movement speed
        var magnitude = Math.Sqrt(totalForceX * totalForceX + totalForceY * totalForceY);
        if (magnitude > 0)
        {
            totalForceX = (totalForceX / magnitude) * 600;
            totalForceY = (totalForceY / magnitude) * 600;
        }

        var targetX = dronePosition.X + (int)totalForceX;
        var targetY = dronePosition.Y + (int)totalForceY;

        // If near edge of map, redirect force vertically to avoid getting stuck on the wall
        if (targetX < 0 || targetX > 10000)
        {
            // Redirect horizontal force to vertical (move up)
            totalForceX = 0;

            targetX = Math.Clamp(dronePosition.X, 0, 9999);

            if (targetY < dronePosition.Y)
            {
                Console.Error.WriteLine($"Redirecting force up, targetY: {targetY}, droneY: {dronePosition.Y}");
                targetY = dronePosition.Y - 600;
            }
            else
            {
                Console.Error.WriteLine($"Before Redirecting force down, targetY: {targetY}, droneY: {dronePosition.Y}");
                targetY = dronePosition.Y + 600;
                Console.Error.WriteLine($"After Redirecting force down, targetY: {targetY}, droneY: {dronePosition.Y}");
            }
        }

        // Clamp to map boundaries
        targetX = Math.Clamp(targetX, 0, 9999);
        targetY = Math.Clamp(targetY, 0, 9999);

        return new Point(targetX, targetY);
    }

    private int CalculateLightLevel(Drone drone)
    {
        var lightLevel = 0;

        if (lastRoundTorchUsed.ContainsKey(drone.Id))
        {
            var lastUsed = lastRoundTorchUsed[drone.Id];

            if (round - lastUsed >= 3)
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



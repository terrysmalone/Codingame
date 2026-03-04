namespace _2023_2_FallChallenge_SeabedSecurity;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Threading;

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

                    if (distanceToMonster <= 2000)
                    {
                        Console.Error.WriteLine($"Adding visible monsters to avoid: {monsterId}");
                        monstersToAvoid.Add(monsterId);
                    }
                }
            }

            // Now add close monsters seen last round to the avoid list
            foreach (var creature in creatures)
            {
                if (creature.Type != -1)
                {
                    continue;
                }

                var distanceToMonster = DistanceCalculator.GetDistance(drone.Position, creature.Position);

                Console.Error.WriteLine($"Checking non visible monster {creature.Id} at distance {distanceToMonster} last seen round {creature.LastSeenRound}");

                if (distanceToMonster <= 2500 && round - creature.LastSeenRound <= 2)
                {
                    Console.Error.WriteLine($"Adding non visible monster to avoid: {creature.Id}");
                    monstersToAvoid.Add(creature.Id);
                }
            }

            Console.Error.WriteLine($"Monsters to avoid: {string.Join(", ", monstersToAvoid)}");

            // Get the closest monster to avoid
            //Creature closestMonster = null;
            //var closestDistance = int.MaxValue;
            //foreach (var monsterId in monstersToAvoid)
            //{
            //    var monster = creatures.Find(c => c.Id == monsterId);
            //    var distanceToMonster = DistanceCalculator.GetDistance(drone.Position, monster.Position);
            //    if (distanceToMonster < closestDistance)
            //    {
            //        closestDistance = distanceToMonster;
            //        closestMonster = monster;
            //    }
            //}

            //Console.Error.WriteLine($"Closest monster: {(closestMonster != null ? closestMonster.Id.ToString() : "None")} at distance {closestDistance}");

            //if (closestMonster != null)
            //{
            //    // Move away from the monster
            //    var directionX = drone.Position.X - closestMonster.Position.X;
            //    var directionY = drone.Position.Y - closestMonster.Position.Y;

            //    var targetX = drone.Position.X + directionX;
            //    var targetY = drone.Position.Y + directionY;

            //    action = $"MOVE {targetX} {targetY} {lightLevel} RUNNING AWAY";
            //}  

            if (monstersToAvoid.Count > 0)
            {
                Point avoidAncePosition = CalculateAvoidanceVector(drone.Position, monstersToAvoid);
                action = $"MOVE {avoidAncePosition.X} {avoidAncePosition.Y} {lightLevel} RUNNING AWAY";
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
                // If stored scans >= 4 head to surface
                if (drone.ScannedCreaturesIds.Count >= 3 || MyScannedCreatureIds.Count + drone.ScannedCreaturesIds.Count >= 12)
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

            // Inverse square law - closer monsters have exponentially more influence
            var forceMagnitude = 2000000.0 / (distance * distance);

            // Normalize direction and apply force
            totalForceX += (dx / distance) * forceMagnitude;
            totalForceY += (dy / distance) * forceMagnitude;
        }

        // Normalize and scale to movement speed (600 units typical)
        var magnitude = Math.Sqrt(totalForceX * totalForceX + totalForceY * totalForceY);
        if (magnitude > 0)
        {
            totalForceX = (totalForceX / magnitude) * 600;
            totalForceY = (totalForceY / magnitude) * 600;
        }

        var targetX = dronePosition.X + (int)totalForceX;
        var targetY = dronePosition.Y + (int)totalForceY;

        // If near edge of map, redirect force upward
        const int edgeBuffer = 1000;
        if (targetX < edgeBuffer || targetX > 10000 - edgeBuffer)
        {
            // Redirect horizontal force to vertical (move up)
            totalForceY -= Math.Abs(totalForceX);
            totalForceX = 0;

            targetX = Math.Clamp(dronePosition.X, edgeBuffer, 10000 - edgeBuffer);
            targetY = dronePosition.Y + (int)totalForceY;
        }

        // Clamp to map boundaries
        targetX = Math.Clamp(targetX, 0, 10000);
        targetY = Math.Clamp(targetY, 0, 10000);

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



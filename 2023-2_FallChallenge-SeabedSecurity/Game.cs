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

    private Dictionary<int, bool> earlyGameTracker = new Dictionary<int, bool>();

    internal HashSet<int> MyStoredCreatureIds { get; private set; } = new();
    internal HashSet<int> EnemyStoredCreatureIds { get; private set; } = new();

    private DirectionCalculator _directionCalculator;
    private MonsterPositionCalculator _monsterPositionCalculator;

    internal int visibleCreatureCount;
    internal HashSet<int> VisibleMonsterIds { get; set; } = new();

    internal int round = 0;

    internal Dictionary<int, int> lastRoundTorchUsed = new();

    public Game()
    {
        earlyGameTracker.Add(0, true);
        earlyGameTracker.Add(1, true);
        earlyGameTracker.Add(2, true);
        earlyGameTracker.Add(3, true);
    }
    internal void InitialiseCreatures(List<Creature> allCreatures)
    {
        creatures = allCreatures;
        _directionCalculator = new DirectionCalculator(this);
        _monsterPositionCalculator = new MonsterPositionCalculator(this);
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
        // Logger.AllMonsters(creatures);

        var actions = new List<string>();

        // If a union of stored creatures and currently scanned creatures by both drones is equal to 12, we know all the creatures and can just head to the surface with all of them
        var allKnownCreatures = new HashSet<int>(MyStoredCreatureIds);
        allKnownCreatures.UnionWith(myDrones[0].ScannedCreaturesIds);
        allKnownCreatures.UnionWith(myDrones[1].ScannedCreaturesIds);

        foreach (var drone in myDrones)
        {
            Console.Error.WriteLine($"Drone {drone.Id}");
            var action = string.Empty;

            var lightLevel = CalculateLightLevel(drone);

            HashSet<int> monstersToAvoid = GetMonstersToAvoid(drone);

            //if (monstersToAvoid.Count > 0)
            //{
            //    earlyGame = false;

            //    Point avoidancePosition = CalculateAvoidanceVector(drone.Position, monstersToAvoid);
            //    action = $"MOVE {avoidancePosition.X} {avoidancePosition.Y} {lightLevel} RUNNING AWAY";

            //    actions.Add(action);
            //    continue;
            //}

            if (earlyGameTracker[drone.Id] == true)
            {
                // Drop to 8,000+ then rise to the top
                if (drone.Position.Y >= 8000)
                {
                    earlyGameTracker[drone.Id] = false;
                }

                // Move to the centre of it's side
                int xPos = 0;

                if (drone.Position.X < 5000)
                {
                    xPos = 2000;
                }
                else
                {
                    xPos = 8000;
                }

                // Calculate the exact targetPoint that's exactly 540 along the path from drone.Position to target
                Point targetPoint = DistanceCalculator.GetPointAlongPath(drone.Position, new Point(xPos, 9500), _droneSpeed);

                // Calculate exact next position of monsters to avoid and try to steer clear of them


                foreach (var monsterId in monstersToAvoid)
                {
                    Creature monster = creatures.Find(c => c.Id == monsterId);
                    Console.Error.WriteLine($"Monster {monster.Id} at position {monster.Position.X},{monster.Position.Y}");

                    Point monsterTarget = _monsterPositionCalculator.PredictTargetPosition(monster);

                    Console.Error.WriteLine($"Predicting monster will move to {monsterTarget.X},{monsterTarget.Y}");

                    // Should I avoid it
                    var willPathsConverge = DistanceCalculator.WillPathsConverge(drone.Position, targetPoint, _droneSpeed, monster.Position, monsterTarget, _monsterDashSpeed);
                    
                    if (willPathsConverge)
                    {
                        Console.Error.WriteLine($"Monster is within 500 of target point, adjusting to avoid");

                        if (drone.Position.X < monster.Position.X)
                        {
                            targetPoint.X = drone.Position.X - 600;
                            targetPoint.Y = drone.Position.Y;
                        }
                        else
                        {
                            targetPoint.X = drone.Position.X + 600;
                            targetPoint.Y = drone.Position.Y;
                        }
                    }
                    else
                    {
                        Console.Error.WriteLine($"Monster is not within 500");
                    }
                }


                actions.Add($"MOVE {targetPoint.X} {targetPoint.Y} {lightLevel} EARLY GAME DIVING");
                continue;
            }

            // TODO: Sometimes we'll want to head to surface for other reasons...
            if (allKnownCreatures.Count >= 12)
            {
                actions.Add($"MOVE {drone.Position.X} 500 {lightLevel} HEADING TO SURFACE");
                continue;
            }
            else
            {
                // Move in the direction of the most unscanned fish
                CreatureDirection direction = _directionCalculator.GetBestDirectionFromRadarBlips(drone);

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
                continue;
            }
        }

        round++;

        return actions;
    }

    private HashSet<int> GetMonstersToAvoid(Drone drone)
    {
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
                    monstersToAvoid.Add(monsterId);
                }
            }
        }

        // Now add close monsters seen last round to the avoid list
        //foreach (var creature in creatures)
        //{
        //    if (creature.Type != -1 || creature.IsVisible || creature.LastSeenRound == 0)
        //    {
        //        continue;
        //    }

        //    var distanceToMonster = DistanceCalculator.GetDistance(drone.Position, creature.Position);

        //    if ((round - creature.LastSeenRound == 1 && distanceToMonster <= _droneSpeed + (2 * _monsterDashSpeed) + _captureSize) ||
        //        (round - creature.LastSeenRound == 2 && distanceToMonster <= _droneSpeed + (3 * _monsterDashSpeed) + _captureSize))
        //    {
        //        monstersToAvoid.Add(creature.Id);
        //    }
        //}

        return monstersToAvoid;
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

            if (targetY < dronePosition.Y)
            {
                targetY = dronePosition.Y - 600;
            }
            else
            {
                targetY = dronePosition.Y + 600;
            }
        }

        // If near bottom of map, redirect force horizontally to avoid getting stuck at the bottom
        if (targetY > 10000)
        {
            totalForceY = 0;

            if (targetX < dronePosition.X)
            {
                targetX = dronePosition.X - 600;
            }
            else
            {
                targetX = dronePosition.X + 600;
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

    internal bool IsScannedByMe(int id)
    {
        foreach (var drone in myDrones)
        {
            if (drone.ScannedCreaturesIds.Contains(id))
            {
                return true;
            }
        }

        return false;
    }

    internal Drone GetNearestDrone(Creature monster)
    {
        Drone nearestDrone = null;
        double nearestDistance = double.MaxValue;
        foreach (var drone in myDrones)
        {
            var distance = DistanceCalculator.GetDistance(drone.Position, monster.Position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestDrone = drone;
            }
        }

        foreach (var drone in enemyDrones)
        {
            var distance = DistanceCalculator.GetDistance(drone.Position, monster.Position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestDrone = drone;
            }
        }

        return nearestDrone;
    }
}



namespace _2023_2_FallChallenge_SeabedSecurity;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Transactions;

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

    private static double angleStep = Math.PI / 6; // 30 degrees in radians
    private List<double> _leftDroneDownAlternativeAngles = new List<double> { angleStep, -angleStep, angleStep * 2, -(angleStep * 2), angleStep * 3, -(angleStep * 3), angleStep * 4, -(angleStep * 4), angleStep * 5, -(angleStep * 5), angleStep * 6, -(angleStep * 6) };
    private List<double> _rightDroneDownAlternativeAngles = new List<double> { -angleStep, angleStep, -(angleStep * 2), angleStep * 2, -(angleStep * 3), angleStep * 3, -(angleStep * 4), angleStep * 4, -(angleStep * 5), angleStep * 5, -(angleStep * 6), angleStep * 6 };

    private List<double> _leftDroneUpAlternativeAngles = new List<double> { -angleStep, angleStep, -(angleStep * 2), angleStep * 2, -(angleStep * 3), angleStep * 3, -(angleStep * 4), angleStep * 4, -(angleStep * 5), angleStep * 5, -(angleStep * 6), angleStep * 6 };
    private List<double> _rightDroneUpAlternativeAngles = new List<double> { angleStep, -angleStep, angleStep * 2, -(angleStep * 2), angleStep * 3, -(angleStep * 3), angleStep * 4, -(angleStep * 4), angleStep * 5, -(angleStep * 5), angleStep * 6, -(angleStep * 6) };


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
            Console.Error.WriteLine($"Drone {drone.Id} pos: {drone.Position.X}, {drone.Position.Y}");
            var action = string.Empty;

            var lightLevel = CalculateLightLevel(drone);

            HashSet<int> monstersToAvoid = GetMonstersToAvoid(drone);

            // If we're below 8,000 or there are no more fish below, stop diving
            if (drone.Position.Y >= 8000 || !AreUnscannedFishStillBelow(drone))
            {
                Console.Error.WriteLine($"Drone {drone.Id} - No more unscanned fish below me or I've reached 8000+ depth, heading to surface and ending early game strategy");
                earlyGameTracker[drone.Id] = false;
            }

            if (earlyGameTracker[drone.Id] == true)
            {                

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

                // Calculate the exact targetPoint that's exactly _droneSpeed along the path from drone.Position to target
                Point targetPoint = DistanceCalculator.GetPointAlongPath(drone.Position, new Point(xPos, 9500), _droneSpeed);

                // Get the paths of all monsters we want to avoid and see if any of them will converge with our path to the target point (within 500)

                // If they will converge, adjust the path by 30 degrees either counter clocwise if they're the left drone, or clockwise if they're the right drone to try to avoid the monster. 
                // Then recheck the paths to see if we still converge, if so adjust further until we don't converge or we've adjusted 11 times (which would be a full circle,
                // so at that point we just accept the risk and move towards the target)
                List<double> alternativeAngles = drone.Position.X < 5000 ? _leftDroneDownAlternativeAngles : _rightDroneDownAlternativeAngles;

                targetPoint = AdjustForMonsters(drone, targetPoint, monstersToAvoid, alternativeAngles);

                actions.Add($"MOVE {targetPoint.X} {targetPoint.Y} {lightLevel} EARLY GAME DIVING");
                continue;
            }

            // TODO: Sometimes we'll want to head to surface for other reasons...
            if (allKnownCreatures.Count >= 12)
            {
                // TO DO: Avoid monsters still
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

    private Point AdjustForMonsters(Drone drone, Point targetPoint, HashSet<int> monstersToAvoid, List<double> alternativeAngles)
    {
        double angleStep = Math.PI / 6; // 30 degrees in radians
        bool converged = false;
        int adjustmentCount = 0;
        Point originalTarget = DistanceCalculator.GetPointAlongPath(drone.Position, new Point(targetPoint.X, targetPoint.Y), _droneSpeed);

        List<(Point start, Point end)> monsterPaths = GetAllMonsterPaths(monstersToAvoid);

        while (adjustmentCount < alternativeAngles.Count)
        {
            Console.Error.WriteLine($"Checking for target {targetPoint.X}, {targetPoint.Y}");
            var willPathsConverge = false;
            foreach (var monsterPath in monsterPaths)
            {
                if (DistanceCalculator.WillPathsConverge(drone.Position, targetPoint, monsterPath.start, monsterPath.end))
                {
                    willPathsConverge = true;
                    break;
                }
            }

            if (willPathsConverge)
            {
                Console.Error.WriteLine($"Drone {drone.Id} - Path converges with a monster, adjusting path. Adjustment count: {adjustmentCount}");
                converged = true;
                double dx = originalTarget.X - drone.Position.X;
                double dy = originalTarget.Y - drone.Position.Y;
                double angle = Math.Atan2(dy, dx);

                if (drone.Position.X < 5000)
                {
                    angle += _leftDroneDownAlternativeAngles[adjustmentCount];
                }
                else
                {
                    angle += _rightDroneDownAlternativeAngles[adjustmentCount];
                }

                int newX = drone.Position.X + (int)(Math.Cos(angle) * _droneSpeed);
                int newY = drone.Position.Y + (int)(Math.Sin(angle) * _droneSpeed);

                newX = Math.Clamp(newX, 0, 9999);
                newY = Math.Clamp(newY, 0, 9999);

                targetPoint = new Point(newX, newY);
                adjustmentCount++;
            }
            else
            {
                if (converged)
                {
                    Console.Error.WriteLine($"Drone {drone.Id} - Found a path that doesn't converge with monsters after {adjustmentCount} adjustments.");
                }
                break;
            }
        }

        return targetPoint;
    }

    private bool AreUnscannedFishStillBelow(Drone drone)
    {
        foreach (var creature in drone.CreatureDirections)
        {
            if (creature.Value == CreatureDirection.BL || creature.Value == CreatureDirection.BR)
            {
                if (!IsScannedOrStoredByMe(creature.Key))
                {
                    Console.Error.WriteLine($"Creature {creature.Key} is below me and unscanned");
                    return true;
                }
            }
        }

        return false;
    }

    private bool IsScannedOrStoredByMe(int key)
    {
        if (MyStoredCreatureIds.Contains(key))
        {
            return true;
        }
        foreach (var drone in myDrones)
        {
            if (drone.ScannedCreaturesIds.Contains(key))
            {
                return true;
            }
        }
        return false;
    }

    private List<(Point start, Point end)> GetAllMonsterPaths(HashSet<int> monstersToAvoid)
    {
        var monsterPaths = new List<(Point start, Point end)>();
        foreach (var monsterId in monstersToAvoid)
        {
            Creature monster = creatures.Find(c => c.Id == monsterId);
            Point monsterTarget = _monsterPositionCalculator.PredictTargetPosition(monster);

            monsterPaths.Add((monster.Position, monsterTarget));
        }

        return monsterPaths;
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

        return monstersToAvoid;
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




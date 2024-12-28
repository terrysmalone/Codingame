using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using static System.Collections.Specialized.BitVector32;
using static System.Formats.Asn1.AsnWriter;

namespace WinterChallenge2024;

internal sealed class Game
{
    internal int Width { get; private set; }
    internal int Height { get; private set; }

    internal List<Organism> PlayerOrganisms { get; private set; }
    internal List<Organism> OpponentOrganisms { get; private set; }

    internal ProteinStock PlayerProteinStock { get; private set; }
    internal ProteinStock OpponentProteinStock { get; private set; }
    
    public List<Point> Walls { get; private set; }
    public List<Protein> Proteins { get; private set; }

    private bool[,] _sporerPoints;

    internal bool[,] isBlocked;
    internal bool[,] hasAnyProtein;
    internal bool[,] hasHarvestedProtein;
    internal bool[,] opponentOrgans;
    internal bool[,] opponentOrganEdges;

    private Stopwatch _timer;
    private long _totalTime;
    private long _segmentTime;
    
    private readonly List<Point> _directions = new List<Point>
    {
        new Point(0, 1),
        new Point(0, -1),
        new Point(1, 0),
        new Point(-1, 0)
    };

    internal Game(int width, int height)
    {
        Width = width;
        Height = height;

        PlayerOrganisms = new List<Organism>();
        OpponentOrganisms = new List<Organism>();

        Walls = new List<Point>();
        Proteins = new List<Protein>();
    }

    internal void SetPlayerProteinStock(ProteinStock playerProteins) => PlayerProteinStock = playerProteins;

    internal void SetOpponentProteinStock(ProteinStock opponentProteins) => OpponentProteinStock = opponentProteins;

    internal void SetPlayerOrganisms(List<Organism> playerOrganisms) => PlayerOrganisms = playerOrganisms;

    internal void SetOpponentOrganisms(List<Organism> opponentOrganisms) => OpponentOrganisms = opponentOrganisms;

    internal void SetWalls(List<Point> walls) => Walls = walls;

    internal void SetProteins(List<Protein> proteins) => Proteins = proteins;

    internal List<string> GetActions()
    {
        _totalTime = 0;
        _timer = new Stopwatch();
        _timer.Start();

        _sporerPoints = new bool[Width, Height];

        UpdateMaps();

        DisplayTime("Updated maps");
        
        // TODO: Add an Action struct to prioritise different actions and choose
        // between them

        CheckForHarvestedProtein();
        DisplayTime("Updated check for harvested protein");
   
        List<string> actions = new List<string>();

        // TODO: I still think Harvesting should be the top priority.
        // //    It's currently after sporing

        int maxProteinDistance = 5;
        int minRootSporerDistance = 4;

        if (PlayerOrganisms.Count < 2)
        {
            maxProteinDistance = 1;
            minRootSporerDistance = 3;
        }
        else if (PlayerOrganisms.Count < 3)
        {
            maxProteinDistance = 3;
        }

        foreach (Organism organism in PlayerOrganisms)
        {
            Console.Error.WriteLine("-------------------------------------");
            Console.Error.WriteLine($"Checking organism: {organism.RootId}");
            string action = string.Empty;

            if (string.IsNullOrEmpty(action))
            {
                action = CheckForTentacleAction(organism);
                DisplayTime("Checked for tentacle action");
            }

            (int closestOrgan, List<Point> shortestPath) = 
                GetShortestPathToProtein(organism, Proteins, 2, 10, GrowStrategy.NO_PROTEINS);

            DisplayTime("Checked for shortest path to protein");

            Console.Error.WriteLine($"Closest organ:{closestOrgan}");
            Console.Error.WriteLine($"Shortest path:{shortestPath.Count}");
            if (shortestPath.Count > 0)
            {
                Display.Path(shortestPath);
            }

            if (string.IsNullOrEmpty(action))
            {
                action = CheckForHarvestAction(closestOrgan, shortestPath, maxProteinDistance);
                DisplayTime("Checked for harvest action");
            }

            if (string.IsNullOrEmpty(action))
            {
                Console.Error.WriteLine("Update sporer spawn points");
                UpdateSporerSpawnPoints();
                DisplayTime("Updated sporer spawn points");
                action = CheckForSporeRootAction(organism, minRootSporerDistance);
                DisplayTime("Checked for spore root action");
            }

            if (string.IsNullOrEmpty(action))
            {
                action = CheckForSporerAction(organism, minRootSporerDistance);
                DisplayTime("Checked for sporer action");
            }

            if (string.IsNullOrEmpty(action))
            {
                // We've already pretty much tried this as part of the 
                // Harvester check but do it again now since we're willing 
                // to go a further now
                action = CheckForMovementAction(closestOrgan, shortestPath);
                DisplayTime("Checked for movement action");
            }

            // If we've gotten this far without getting a move things are 
            // desterate. We're either truly blocked or we've blocked ourselves
            // by not wanting to grow over proteins. Try that now
            if (string.IsNullOrEmpty(action))
            {
                action = GetDesperateDestructiveMove(organism, GrowStrategy.UNHARVESTED);
                DisplayTime("Checked for desperate action");
            }

            // We're even more desperate now. Lets consider growing on harvested 
            // proteins
            if (string.IsNullOrEmpty(action))
            {
                action = GetDesperateDestructiveMove(organism, GrowStrategy.ALL_PROTEINS);
                DisplayTime("Checked for very desperate action");
            }

            // If there wasn't a protein to go to just spread randomly...for now
            if (string.IsNullOrEmpty(action) &&
                CostCalculator.CanProduceOrgan(OrganType.BASIC, PlayerProteinStock))
            {                
                action = GetRandomBasicGrow(organism);
                DisplayTime("Checked for random move action");
            }

            if (string.IsNullOrEmpty(action))
            {
                action = "WAIT";
            }

            actions.Add(action);
        }

        DisplayTime("Done");

        _timer.Stop();

        return actions;
    }

    private void DisplayTime(string message)
    {
        long segmentTime = _timer.ElapsedTicks;
        _totalTime += segmentTime;
        Display.TimeStamp(_totalTime, segmentTime, message);
        _timer.Restart();
    }

    private string CheckForTentacleAction(Organism organism)
    {
        if (CostCalculator.CanProduceOrgan(OrganType.TENTACLE, PlayerProteinStock))
        {    
            foreach (Organ organ in organism.Organs)
            {
                // Console.Error.WriteLine($"Checking organ: {organ.Id}");

                Point organPoint = organ.Position;

                foreach (Point direction in _directions)
                {
                    Point checkPoint = new Point(organPoint.X + direction.X,
                                                 organPoint.Y + direction.Y);

                    // Console.Error.WriteLine($"Checking point {checkPoint.X},{checkPoint.Y}");

                    // TODO: I can't grow a tentacle in front of an opponent tentacle
                    //       THis can be updated in the opponent edges map
                    if (MapChecker.CanGrowOn(checkPoint,
                                             this,
                                             GrowStrategy.ALL_PROTEINS))
                    {
                        // Console.Error.WriteLine("Can grow");
                        if (opponentOrganEdges[checkPoint.X, checkPoint.Y])
                        {
                            // Console.Error.WriteLine("FOUND OPPONENT EDGE");

                            string dir = string.Empty;

                            if (checkPoint.Y - 1 >= 0 && opponentOrgans[checkPoint.X, checkPoint.Y - 1])
                            {
                                dir = "N";
                            }
                            else if (checkPoint.X + 1 < Width && opponentOrgans[checkPoint.X + 1, checkPoint.Y])
                            {
                                dir = "E";
                            }
                            else if (checkPoint.Y + 1 < Height && opponentOrgans[checkPoint.X, checkPoint.Y + 1])
                            {
                                dir = "S";
                            }
                            else if (checkPoint.X - 1 >= 0 && opponentOrgans[checkPoint.X-1, checkPoint.Y])
                            {
                                dir = "W";
                            }

                            return $"GROW {organ.Id} {checkPoint.X} {checkPoint.Y} TENTACLE {dir}";
                        }
                    }
                }
            }
        }

        return string.Empty;
    }

    internal void UpdateMaps()
    {
        // Reset them all at the start because some of the calculation 
        // will make changes to the others.
        isBlocked = new bool[Width, Height];

        hasHarvestedProtein = new bool[Width, Height];
        hasAnyProtein = new bool[Width, Height];

        opponentOrgans = new bool[Width, Height];
        opponentOrganEdges = new bool[Width, Height];

        UpdateIsBlocked();
        UpdateHasProteins();
        UpdateOpponentOrgans();
    }

    private void UpdateOpponentOrgans()
    {
        foreach (Organism organism in OpponentOrganisms)
        {
            foreach (Organ organ in organism.Organs)
            {
                opponentOrgans[organ.Position.X, organ.Position.Y] = true;

                // We can't walk on an outward facing tentacle
                // So add these to the isBlocked list and not to the valid edges

                // North
                if (organ.Position.Y - 1 >= 0)
                {
                    if (organ.Type == OrganType.TENTACLE && organ.Direction == OrganDirection.N)
                    {
                        isBlocked[organ.Position.X, organ.Position.Y - 1] = true;
                    }
                    else
                    {
                        opponentOrganEdges[organ.Position.X, organ.Position.Y - 1] = true;
                    }
                }

                // East
                if (organ.Position.X + 1 < Width)
                {
                    if (organ.Type == OrganType.TENTACLE && organ.Direction == OrganDirection.E)
                    {
                        isBlocked[organ.Position.X + 1, organ.Position.Y] = true;
                    }
                    else
                    {
                        opponentOrganEdges[organ.Position.X + 1, organ.Position.Y] = true;
                    }
                }

                // South
                if (organ.Position.Y + 1 < Height)
                {
                    if (organ.Type == OrganType.TENTACLE && organ.Direction == OrganDirection.S)
                    {
                        isBlocked[organ.Position.X, organ.Position.Y + 1] = true;
                    }
                    else
                    {
                        opponentOrganEdges[organ.Position.X, organ.Position.Y + 1] = true;
                    }
                }

                // WEST
                if (organ.Position.X - 1 >= 0)
                {
                    if (organ.Type == OrganType.TENTACLE && organ.Direction == OrganDirection.W)
                    {
                        isBlocked[organ.Position.X - 1, organ.Position.Y] = true;
                    }
                    else
                    {
                        opponentOrganEdges[organ.Position.X - 1, organ.Position.Y] = true;
                    }
                }
            }
        }
    }

    private void UpdateIsBlocked()
    {
        // Not walkable if player organ on that spot
        foreach (Organism organism in PlayerOrganisms)
        {
            foreach (Organ organ in organism.Organs)
            {
                isBlocked[organ.Position.X, organ.Position.Y] = true;
            }
        }

        // Not walkable if opponent organ on that spot
        foreach (Organism organism in OpponentOrganisms)
        {
            foreach (Organ organ in organism.Organs)
            {
                isBlocked[organ.Position.X, organ.Position.Y] = true;
            }
        }

        // Not walkable if wall on that spot
        foreach (Point wall in Walls)
        {
            isBlocked[wall.X, wall.Y] = true;
        }
    }

    private void UpdateHasProteins()
    {
        foreach (Protein protein in Proteins)
        {
            if (protein.IsHarvested)
            {
                hasHarvestedProtein[protein.Position.X, protein.Position.Y] = true;
                hasAnyProtein[protein.Position.X, protein.Position.Y] = true;
            }
            else
            {
                hasAnyProtein[protein.Position.X, protein.Position.Y] = true;
            }
        }
    }

    private void UpdateSporerSpawnPoints()
    {
        foreach (Protein protein in Proteins.Where(p => !p.IsHarvested))
        {
            List<Point> possibleRootPoints = MapChecker.GetRootPoints(protein.Position, this);
            foreach (var possPoint in possibleRootPoints)
            {
                if (!MapChecker.HasNearbyOrgan(possPoint, PlayerOrganisms))
                {
                    _sporerPoints[possPoint.X, possPoint.Y] = true;
                }
            }

            // Console.Error.WriteLine($"{possibleRootPoints.Count} possible root points added for protein {protein.Position.X},{protein.Position.Y}");
        }
    }

    // Check to see if any protein is being harvested and mark it as such
    private void CheckForHarvestedProtein()
    {
        foreach (Organism organism in PlayerOrganisms)
        {
            foreach (Organ organ in organism.Organs)
            {
                if (organ.Type == OrganType.HARVESTER)
                {
                    Point harvestedPosition = GetHarvestedPosition(organ);

                    if(Proteins.Any(p => p.Position == harvestedPosition))
                    {
                        Protein havestedProtein = Proteins.Single(p => p.Position == harvestedPosition);

                        havestedProtein.IsHarvested = true;
                    }
                }
            }
        }

        // We don't care about enemy harvested proteins because
        // we're still happy to consume them.
    }

    private string CheckForHarvestAction(int closestOrgan, List<Point> shortestPath, int maxProteinDistance)
    {
        string action = string.Empty;

        if (closestOrgan != -1)
        {
            // TODO: If we can't harvest, check if we can consume the organisms needed
            //       to create a harvester

            // See if we can make a harvester
            if (CostCalculator.CanProduceOrgan(OrganType.HARVESTER, PlayerProteinStock) &&
                Proteins.Exists(p => p.IsHarvested == false))
            {
                if (shortestPath.Count == 2)
                {
                    string dir = GetDirection(shortestPath[0], shortestPath[1]);

                    action = $"GROW {closestOrgan} {shortestPath[0].X} {shortestPath[0].Y} HARVESTER {dir}";
                }
            }

            int maxWalkingDistance = Math.Min(maxProteinDistance, PlayerProteinStock.A);

            if (string.IsNullOrEmpty(action) && (shortestPath.Count <= maxWalkingDistance))
            {
                // Grow towards the nearest protein
                // Grow towards the nearest protein
                string direction = CalculateClosestOpponentDirection(OpponentOrganisms, shortestPath[0]);

                // If we can make it a tentacle and still have some spare proteins then do it
                if (CostCalculator.CanProduceOrgan(OrganType.TENTACLE, PlayerProteinStock, 5))
                {
                    action = $"GROW {closestOrgan} {shortestPath[0].X} {shortestPath[0].Y} TENTACLE {direction}";
                }
                else if (CostCalculator.CanProduceOrgan(OrganType.BASIC, PlayerProteinStock))
                {
                    action = $"GROW {closestOrgan} {shortestPath[0].X} {shortestPath[0].Y} BASIC";
                }
            }
        }

        return action;
    }

    private string CheckForSporerAction(Organism organism, int minRootSporerDistance)
    {
        string action = string.Empty;

        if (CostCalculator.CanProduceOrgan(OrganType.ROOT, PlayerProteinStock) &&
            CostCalculator.CanProduceOrgan(OrganType.SPORER, PlayerProteinStock))
        {
            int furthestDistance = -1;
            int furthestOrgan = -1;
            Point furthestSporerPoint = new Point(0, 0);
            string furthestDirection = string.Empty;

            // for each organ
            foreach (Organ organ in organism.Organs)
            {
                Point organPoint = organ.Position;
                List<Point> directions = new List<Point>();

                // Check south
                if (organPoint.Y <= Height - minRootSporerDistance - 1)
                {
                    directions.Add(new Point(0, 1));
                }

                // Check North
                if (organPoint.Y >= minRootSporerDistance)
                {
                    directions.Add(new Point(0, -1));
                }

                // Check East
                if (organPoint.X <= Width - minRootSporerDistance - 1)
                {
                    directions.Add(new Point(1, 0));
                }

                // Check West
                if (organPoint.X >= minRootSporerDistance)
                {
                    directions.Add(new Point(-1, 0));
                }

                // Check the four points around the organ
                foreach (Point side in directions)
                {
                    Point sporerPoint = new Point(organPoint.X + side.X,
                                                  organPoint.Y + side.Y);

                    if (!MapChecker.CanGrowOn(sporerPoint, this, GrowStrategy.NO_PROTEINS))
                    {
                        continue;
                    }

                    // Check in all 4 directions
                    foreach (Point direction in directions)
                    {
                        Point checkPoint = new Point(sporerPoint.X,
                                                     sporerPoint.Y);

                        int distance = 1;
                        bool pathClear = true;
                        while (pathClear)
                        {
                            checkPoint = new Point(checkPoint.X + direction.X,
                                                   checkPoint.Y + direction.Y);

                            // Console.Error.WriteLine($"Checking point {checkPoint.X},{checkPoint.Y}");

                            if (checkPoint.X < 0) { break; }

                            if (checkPoint.X >= Width) { break; }

                            if (checkPoint.Y < 0) { break; }

                            if (checkPoint.Y >= Height) { break; }

                            if (distance >= minRootSporerDistance)
                            {
                                // Console.Error.WriteLine($"Distance viable");
                                //    if it's on a spawn point 
                                if (_sporerPoints[checkPoint.X, checkPoint.Y])
                                {
                                    // Console.Error.WriteLine($"There's a spore point");
                                    string dir = string.Empty;

                                    if (direction.X == 1)
                                    {
                                        dir = "E";
                                    }
                                    else if (direction.X == -1)
                                    {
                                        dir = "W";
                                    }
                                    else if (direction.Y == -1)
                                    {
                                        dir = "N";
                                    }
                                    else if (direction.Y == 1)
                                    {
                                        dir = "S";
                                    }

                                    if (distance > furthestDistance)
                                    {
                                        //Console.Error.WriteLine("Added to furthestDistance");
                                        furthestDistance = distance;
                                        furthestOrgan = organ.Id;
                                        furthestSporerPoint = new Point(sporerPoint.X, sporerPoint.Y);
                                        furthestDirection = dir;
                                    }
                                }
                            }

                            if (!MapChecker.CanGrowOn(checkPoint, this, GrowStrategy.ALL_PROTEINS))
                            {
                                pathClear = false;
                            }

                            distance++;
                        }
                    }
                }
            }

            if (furthestDistance != -1)
            {
                return $"GROW {furthestOrgan} {furthestSporerPoint.X} {furthestSporerPoint.Y} SPORER {furthestDirection}";

            }
        }

        return string.Empty;
    }

    private string CheckForSporeRootAction(Organism organism, int minRootSporerDistance)
    {
        if (organism.Organs.Any(o => o.Type == OrganType.SPORER) &&
                CostCalculator.CanProduceOrgan(OrganType.ROOT, PlayerProteinStock))
        {
            List<Organ> sporers = organism.Organs.Where(o => o.Type == OrganType.SPORER).ToList();

            int furthestDistance = -1;
            int furthestSporerId = -1;
            Point furthestRootPoint = new Point(0, 0);

            foreach (Organ sporer in sporers)
            {
                Point direction = new Point(0, 0);

                switch (sporer.Direction)
                {
                    case OrganDirection.N:
                        direction = new Point(0, -1);
                        break;
                    case OrganDirection.E:
                        direction = new Point(1, 0);
                        break;
                    case OrganDirection.S:
                        direction = new Point(0, 1);
                        break;
                    case OrganDirection.W:
                        direction = new Point(-1, 0);
                        break;
                }

                if (direction == new Point(0, 0))
                {
                    Console.Error.WriteLine($"ERROR: Couldn't get sporer direction for {sporer.Position.X}{sporer.Position.Y}");
                }

                Point checkPoint = new Point(sporer.Position.X, sporer.Position.Y);

                int distance = 1;
                bool pathClear = true;
                while (pathClear)
                {
                    checkPoint = new Point(checkPoint.X + direction.X,
                                           checkPoint.Y + direction.Y);

                    // Console.Error.WriteLine($"checkPoint {checkPoint.X},{checkPoint.Y}");

                    if (checkPoint.X < 0) { break; }

                    if (checkPoint.X >= Width) { break; }

                    if (checkPoint.Y < 0) { break; }

                    if (checkPoint.Y >= Height) { break; }

                    if (distance >= minRootSporerDistance)
                    {
                        Console.Error.WriteLine($"Distance viable");
                        //    if it's on a spawn point 
                        if (_sporerPoints[checkPoint.X, checkPoint.Y])
                        {
                            if (distance > furthestDistance)
                            {
                                furthestDistance = distance;
                                furthestSporerId = sporer.Id;
                                furthestRootPoint = checkPoint;
                            }
                        }
                    }

                    if (!MapChecker.CanGrowOn(checkPoint, this, GrowStrategy.ALL_PROTEINS))
                    {
                        Console.Error.WriteLine($"Path not clear");

                        pathClear = false;
                    }

                    distance++;
                }
            }

            if (furthestDistance != -1)
            {
                return $"SPORE {furthestSporerId} {furthestRootPoint.X} {furthestRootPoint.Y}";
            }
        }

        return string.Empty;
    }

    private string CheckForMovementAction(int closestOrgan, List<Point> shortestPath)
    {
        string action = string.Empty;

        if (closestOrgan != -1)
        {
            // Grow towards the nearest protein
            string direction = CalculateClosestOpponentDirection(OpponentOrganisms, shortestPath[0]);

            // If we can make it a tentacle and still have some spare proteins then do it
            if (CostCalculator.CanProduceOrgan(OrganType.TENTACLE, PlayerProteinStock, 5))
            {
                action = $"GROW {closestOrgan} {shortestPath[0].X} {shortestPath[0].Y} TENTACLE {direction}";
            }
            else if (CostCalculator.CanProduceOrgan(OrganType.BASIC, PlayerProteinStock))
            {
                action = $"GROW {closestOrgan} {shortestPath[0].X} {shortestPath[0].Y} BASIC";
            }
            else if (CostCalculator.CanProduceOrgan(OrganType.SPORER, PlayerProteinStock))
            {
                action = $"GROW {closestOrgan} {shortestPath[0].X} {shortestPath[0].Y} SPORER {direction}";
            }
            else if (CostCalculator.CanProduceOrgan(OrganType.HARVESTER, PlayerProteinStock))
            {
                action = $"GROW {closestOrgan} {shortestPath[0].X} {shortestPath[0].Y} HARVESTER {direction}";
            }
        }

        return action;
    }

    private static Point GetHarvestedPosition(Organ organ)
    {
        switch (organ.Direction)
        {
            case OrganDirection.N:
                return new Point(organ.Position.X, organ.Position.Y-1);
            case OrganDirection.E:
                return new Point(organ.Position.X+1, organ.Position.Y);
            case OrganDirection.S:
                return new Point(organ.Position.X, organ.Position.Y+1);
            case OrganDirection.W:
                return new Point(organ.Position.X-1, organ.Position.Y);
        }

        return new Point(-1,-1);
    }

    private string GetDirection(Point from, Point to)
    {
        string dir = "N";

        if (from.X < to.X)
        {
            dir = "E";
        }
        else if (from.X > to.X)
        {
            dir = "W";
        }
        else if (from.Y < to.Y)
        {
            dir = "S";
        }

        return dir;
    }

    private (int,List<Point>) GetShortestPathToProtein(Organism organism, List<Protein> proteins, int minDistance, int maxDistance, GrowStrategy growStrategy)
    {
        string action = string.Empty;

        int shortest = int.MaxValue;
        int closestId = -1;
        List<Point> shortestPath = new List<Point>();

        AStar aStar = new AStar(this);

        // Get the closest protein to Organs
        foreach (Protein protein in proteins)
        {
            // Console.Error.WriteLine($"Checking protein: {protein.Position.X},{protein.Position.Y}");
   
            if (protein.IsHarvested)
            {
                continue;
            }

            foreach (var organ in organism.Organs)
            {
                // Console.Error.WriteLine($"Checking organ: {organ.Position.X},{organ.Position.Y}");
                
                int manhattanDistance = MapChecker.CalculateManhattanDistance(organ.Position, protein.Position);

                // Console.Error.WriteLine($"Manhattan distance: {manhattanDistance}");
                // Console.Error.WriteLine($"Max distance: {maxDistance}");
                
                if (manhattanDistance > maxDistance)
                {
                    continue;
                }

                List<Point> path = aStar.GetShortestPath(organ.Position, protein.Position, maxDistance, growStrategy);

                    // Console.Error.WriteLine($"Shortest path count: {path.Count}");
                    //Display.Path(shortestPath);

                if (path.Count < shortest && path.Count >= minDistance && path.Count != 0)
                {
                    shortest = path.Count;
                    shortestPath = new List<Point>(path);

                    closestId = organ.Id;

                    if (shortest < maxDistance)
                    {
                        maxDistance = shortest;
                    }
                }
            }
        }

        return (closestId, shortestPath);
    }

    private string GetRandomBasicGrow(Organism organism)
    {
        string action = string.Empty;

        for (int i = organism.Organs.Count - 1; i >= 0; i--)
        {
            Organ current = organism.Organs[i];

            foreach (Point direction in _directions)
            {
                Point checkPoint = new Point(current.Position.X + direction.X, 
                                             current.Position.Y + direction.Y);

                if (MapChecker.CanGrowOn(checkPoint, this, GrowStrategy.ALL_PROTEINS))
                {
                    action = $"GROW {current.Id} {checkPoint.X} {checkPoint.Y} BASIC";

                    // TODO: Let us choose other nodes too
                    break;
                }
            }
        }

        return action;
    }

    private string GetDesperateDestructiveMove(Organism organism, GrowStrategy growStrategy)
    {
        // Display.Organisms(PlayerOrganisms);
        // TODO: I want max distance to be 2 here but then it bugs out
        (int closestOrgan, List<Point> shortestPath) = GetShortestPathToProtein(organism, Proteins, 1, 10, growStrategy);

        if (closestOrgan != -1)
        {
            string closestRootDirection = CalculateClosestOpponentDirection(OpponentOrganisms, shortestPath[0]);

            string organToGrow = string.Empty;
            if (CostCalculator.CanProduceOrgan(OrganType.TENTACLE, PlayerProteinStock))
            {
                organToGrow = $"{OrganType.TENTACLE.ToString()} {closestRootDirection}";
            }
            else if (CostCalculator.CanProduceOrgan(OrganType.BASIC, PlayerProteinStock))
            {
                organToGrow = OrganType.BASIC.ToString();
            }
            else if (CostCalculator.CanProduceOrgan(OrganType.SPORER, PlayerProteinStock))
            {
                organToGrow = $"{OrganType.SPORER.ToString()} {closestRootDirection}";
            }
            else if (CostCalculator.CanProduceOrgan(OrganType.HARVESTER, PlayerProteinStock))
            {
                organToGrow = $"{OrganType.HARVESTER.ToString()} {closestRootDirection}";
            }

            if (string.IsNullOrEmpty(organToGrow))
            {
                return string.Empty;
            }

            return $"GROW {closestOrgan} {shortestPath[0].X} {shortestPath[0].Y} {organToGrow}";
        
            // TODO: IF we can't afford a basic just make another organ type
        }

        return string.Empty;
    }

    // Calculates the direction of the closest enemy root to a given point.
    private string CalculateClosestOpponentDirection(List<Organism> opponentOrganisms, Point startPoint)
    {
        Point endPoint = GetClosestRoot(opponentOrganisms, startPoint);
        
        if (Math.Abs(endPoint.X - startPoint.X) >= Math.Abs(endPoint.Y - startPoint.Y))
        {
            // It's either east or west
            if (endPoint.X > startPoint.X)
            {
                return "E";
            }
            else
            {
                return "W";
            }
        }
        else
        {
            // It's either north or south
            if (endPoint.Y > startPoint.Y)
            {
                return "S";
            }
            else
            {
                return "N";
            }
        }
    }

    private Point GetClosestRoot(List<Organism> opponentOrganisms, Point startPoint)
    {
        int closestDistance = int.MaxValue;
        Point closestPoint = new Point(-1, -1);

        foreach (Organism opponentOrganism in OpponentOrganisms)
        {
            Organ root = opponentOrganism.Organs.Single(o => o.Type == OrganType.ROOT);

            int distance = MapChecker.CalculateManhattanDistance(root.Position, startPoint);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPoint = root.Position;
            }
        }

        return closestPoint;
    }
}

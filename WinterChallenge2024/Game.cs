using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
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
    int _minRootSporerDistance = 4;

    internal bool[,] isBlocked;
    internal bool[,] hasAnyProtein;
    internal bool[,] hasHarvestedProtein;

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

    int turn = 0;
    internal List<string> GetActions()
    {
        _sporerPoints = new bool[Width, Height];

        UpdateIsBlocked();
        UpdateHasProteins();
      
        // TODO: Add an Action struct to prioritise different actions and choose
        // between them

        CheckForHarvestedProtein();
        
        List<string> actions = new List<string>();

        // TODO: I still think HArvesting should be the top priority.
        // //    It's currently after sporing

        foreach (Organism organism in PlayerOrganisms)
        {
            string action = string.Empty;

            Console.Error.WriteLine("GetShortestPathToProtein");
            (int closestOrgan, List<Point> shortestPath) = GetShortestPathToProtein(organism, Proteins, 2, 10, GrowStrategy.NO_PROTEINS);

            Console.Error.WriteLine($"Closest organ:{closestOrgan}");
            Display.Path(shortestPath);

            Console.Error.WriteLine("Got closest path");
            if (string.IsNullOrEmpty(action))
            {
                Console.Error.WriteLine("CheckForHarvestAction");
                action = CheckForHarvestAction(closestOrgan, shortestPath);
            }


            if (string.IsNullOrEmpty(action))
            {
                Console.Error.WriteLine("Update sporer spawn points");
                UpdateSporerSpawnPoints();
                Console.Error.WriteLine("CheckForSporeRootAction");
                action = CheckForSporeRootAction(organism);
            }

            if (string.IsNullOrEmpty(action))
            {
                Console.Error.WriteLine("CheckForSporerAction");
                action = CheckForSporerAction(organism);
            }

            if (string.IsNullOrEmpty(action))
            {
                // We've already pretty much tried this as part of the 
                // Harvester check but do it again now since we're willing 
                // to go a further now
                Console.Error.WriteLine("CheckForBasicAction");
                action = CheckForBasicAction(closestOrgan, shortestPath);
            }

            // If we've gotten this far without getting a move things are 
            // desterate. We're either truly blocked or we've blocked ourselves
            // by not wanting to grow over proteins. Try that now
            if (string.IsNullOrEmpty(action))
            {
                Console.Error.WriteLine("GetDesperateDestructiveMove");
                action = GetDesperateDestructiveMove(organism, GrowStrategy.UNHARVESTED);
            }

            // We're even more desperate now. Lets consider growing on harvested 
            // proteins
            if (string.IsNullOrEmpty(action))
            {
                Console.Error.WriteLine("GetEvenMoreDesperateDestructiveMove");
                action = GetDesperateDestructiveMove(organism, GrowStrategy.ALL_PROTEINS);
            }

            // If there wasn't a protein to go to just spread randomly...for now
            if (string.IsNullOrEmpty(action) &&
                CostCalculator.CanProduceOrgan(OrganType.BASIC, PlayerProteinStock))
            {
                //Console.Error.WriteLine("GetRandomBasicGrow");
                action = GetRandomBasicGrow(organism);
            }

            if (string.IsNullOrEmpty(action))
            {
                action = "WAIT";
            }

            actions.Add(action);
        }

        return actions;
    }

    internal void UpdateIsBlocked()
    {
        isBlocked = new bool[Width, Height];
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

    internal void UpdateHasProteins()
    {
        hasHarvestedProtein = new bool[Width, Height];
        hasAnyProtein = new bool[Width, Height];

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
            if (MapChecker.HasNearbyOrgan(protein, PlayerOrganisms))
            {
                continue;
            }

            List<Point> possibleRootPoints = MapChecker.GetRootPoints(protein.Position, this);
            foreach (var possPoint in possibleRootPoints)
            {
                _sporerPoints[possPoint.X, possPoint.Y] = true;
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

    private string CheckForHarvestAction(int closestOrgan, List<Point> shortestPath)
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

            int maxWalkingDistance = Math.Min(5, PlayerProteinStock.A);

            if (string.IsNullOrEmpty(action) && (shortestPath.Count <= maxWalkingDistance))
            {
                // Grow towards the nearest protein
                if (CostCalculator.CanProduceOrgan(OrganType.BASIC, PlayerProteinStock))
                {
                    action = $"GROW {closestOrgan} {shortestPath[0].X} {shortestPath[0].Y} BASIC";
                }
            }
        }

        return action;
    }

    private string CheckForSporerAction(Organism organism)
    {
        string action = string.Empty;

        if (CostCalculator.CanProduceOrgan(OrganType.ROOT, PlayerProteinStock) &&
            CostCalculator.CanProduceOrgan(OrganType.SPORER, PlayerProteinStock))
        {
            // for each organ
            foreach (Organ organ in organism.Organs)
            {
                Point organPoint = organ.Position;

                //    for each direction
                List<Point> directions = new List<Point>();

                // Check south
                if (organPoint.Y <= Height - _minRootSporerDistance - 1)
                {
                    directions.Add(new Point(0, 1));
                }

                // Check North
                if (organPoint.Y >= _minRootSporerDistance)
                {
                    directions.Add(new Point(0, -1));
                }

                // Check East
                if (organPoint.X <= Width - _minRootSporerDistance - 1)
                {
                    directions.Add(new Point(1, 0));
                }

                // Check West
                if (organPoint.X >= _minRootSporerDistance)
                {
                    directions.Add(new Point(-1, 0));
                }

                foreach (Point direction in directions)
                {
                    Point sporerPoint = new Point(organPoint.X + direction.X, 
                                                  organPoint.Y + direction.Y);

                    if (!MapChecker.CanGrowOn(sporerPoint, GrowStrategy.NO_PROTEINS, this))
                    {
                        continue;
                    }

                    Point checkPoint = new Point(sporerPoint.X, sporerPoint.Y);

                    int distance = 1;
                    bool pathClear = true;
                    while (pathClear)
                    {
                        checkPoint = new Point(checkPoint.X + direction.X,
                                               checkPoint.Y + direction.Y);
                        
                        Console.Error.WriteLine($"checkPoint {checkPoint.X},{checkPoint.Y}");

                        if (checkPoint.X < 0) { break; }

                        if (checkPoint.X >= Width) { break; }

                        if (checkPoint.Y < 0) { break; }

                        if (checkPoint.Y >= Height) { break; }

                        if (distance >= _minRootSporerDistance)
                        {
                            Console.Error.WriteLine($"Distance viable");
                            //    if it's on a spawn point 
                            if (_sporerPoints[checkPoint.X, checkPoint.Y])
                            {
                                Console.Error.WriteLine($"There's a spore point");
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

                                return $"GROW {organ.Id} {sporerPoint.X} {sporerPoint.Y} SPORER {dir}";
                            }
                        }

                        if (!MapChecker.CanGrowOn(checkPoint, GrowStrategy.ALL_PROTEINS, this))
                        {
                            Console.Error.WriteLine($"Path not clear");

                            pathClear = false;
                        }

                        distance++;
                    }
                }
            }
        }

        return string.Empty;
    }

    private string CheckForSporeRootAction(Organism organism)
    {
        if (organism.Organs.Any(o => o.Type == OrganType.SPORER) &&
                CostCalculator.CanProduceOrgan(OrganType.ROOT, PlayerProteinStock))
        {
            Organ sporer = new Organ();
            List<Organ> sporers = organism.Organs.Where(o => o.Type == OrganType.SPORER).ToList();

            if (sporers.Count == 0)
            {
                return string.Empty;
            }
            else if (sporers.Count == 1)
            {
                sporer = sporers[0];
            }
            else
            {
                bool assigned = false;
                foreach (Organ o in sporers)
                {
                    if (!MapChecker.HasSporerSpored(o, this))
                    {
                        sporer = o;
                        assigned = true;
                    }
                }

                if (!assigned)
                {
                    return string.Empty;
                }
            }

            Console.Error.WriteLine($"Selected sporer is {sporer.Id}");

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

            if (direction == new Point(0,0))
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

                Console.Error.WriteLine($"checkPoint {checkPoint.X},{checkPoint.Y}");

                if (checkPoint.X < 0) { break; }

                if (checkPoint.X >= Width) { break; }

                if (checkPoint.Y < 0) { break; }

                if (checkPoint.Y >= Height) { break; }

                if (distance >= _minRootSporerDistance)
                {
                    Console.Error.WriteLine($"Distance viable");
                    //    if it's on a spawn point 
                    if (_sporerPoints[checkPoint.X, checkPoint.Y])
                    {
                        return $"SPORE {sporer.Id} {checkPoint.X} {checkPoint.Y}";
                    }
                }

                if (!MapChecker.CanGrowOn(checkPoint, GrowStrategy.ALL_PROTEINS, this))
                {
                    Console.Error.WriteLine($"Path not clear");

                    pathClear = false;
                }

                distance++;
            }
        }

        return string.Empty;
    }

    private string CheckForBasicAction(int closestOrgan, List<Point> shortestPath)
    {
        string action = string.Empty;

        if (closestOrgan != -1)
        {
            // If not, just grow towards the nearest A protein
            if (string.IsNullOrEmpty(action) &&
                CostCalculator.CanProduceOrgan(OrganType.BASIC, PlayerProteinStock))
            {
                action = $"GROW {closestOrgan} {shortestPath[0].X} {shortestPath[0].Y} BASIC";
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
            bool debug = false;

            if (protein.Position == new Point(3, 7))
            {
                debug = true;
            }

            if (debug)
            {
                Console.Error.WriteLine($"Checking protein: {protein.Position.X},{protein.Position.Y}");
            }

            if (protein.IsHarvested)
            {
                continue;
            }

            foreach (var organ in organism.Organs)
            {
                if (debug)
                {
                    Console.Error.WriteLine($"Checking organ: {organ.Position.X},{organ.Position.Y}");
                }

                int manhattanDistance = MapChecker.CalculateManhattanDistance(organ.Position, protein.Position);

                if (debug)
                {
                    Console.Error.WriteLine($"Manhattan distance: {manhattanDistance}");
                    Console.Error.WriteLine($"Max distance: {maxDistance}");
                }

                if (manhattanDistance > maxDistance)
                {
                    continue;
                }

                List<Point> path = aStar.GetShortestPath(organ.Position, protein.Position, maxDistance, growStrategy);

                if (debug)
                {
                    Console.Error.WriteLine($"Shortest path count: {path.Count}");
                    Display.Path(shortestPath);
                }

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

            if (MapChecker.CanGrowOn(new Point(current.Position.X + 1, current.Position.Y), this))
            {
                action = $"GROW {current.Id} {current.Position.X + 1} {current.Position.Y} BASIC";
                break;
            }

            if (MapChecker.CanGrowOn(new Point(current.Position.X, current.Position.Y + 1), this))
            {
                action = $"GROW {current.Id} {current.Position.X} {current.Position.Y + 1} BASIC";
                break;
            }

            if (MapChecker.CanGrowOn(new Point(current.Position.X, current.Position.Y - 1), this))
            {
                action = $"GROW {current.Id} {current.Position.X} {current.Position.Y - 1} BASIC";
                break;
            }

            if (MapChecker.CanGrowOn(new Point(current.Position.X - 1, current.Position.Y), this))
            {
                action = $"GROW {current.Id} {current.Position.X - 1} {current.Position.Y} BASIC";
                break;
            }
        }

        return action;
    }

    private string GetDesperateDestructiveMove(Organism organism, GrowStrategy growStrategy)
    {
        Display.Organisms(PlayerOrganisms);
        // TODO: I want max distance to be 2 here but then it bugs out
        (int closestOrgan, List<Point> shortestPath) = GetShortestPathToProtein(organism, Proteins, 1, 10, growStrategy);

        if (closestOrgan != -1)
        {
            string organToGrow = string.Empty;
            if (CostCalculator.CanProduceOrgan(OrganType.BASIC, PlayerProteinStock))
            {
                organToGrow = OrganType.BASIC.ToString();
            }
            else if (CostCalculator.CanProduceOrgan(OrganType.TENTACLE, PlayerProteinStock))
            {
                // TODO: Maybe pick a direction
                organToGrow = OrganType.TENTACLE.ToString() + " E";
            }
            else if (CostCalculator.CanProduceOrgan(OrganType.HARVESTER, PlayerProteinStock))
            {
                // TODO: Maybe pick a direction
                organToGrow = OrganType.HARVESTER.ToString() + " E";
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
}

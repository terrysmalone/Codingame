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

    internal Game(int width, int height)
    {
        Width = width;
        Height = height;
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
        // TODO: Add an Action struct to prioritise different actions and choose
        // between them

        CheckForHarvestedProtein();
        
        List<string> actions = new List<string>();

        foreach (Organism organism in PlayerOrganisms)
        {
            string action = string.Empty;

            if (string.IsNullOrEmpty(action))
            {
                action = CheckForSporeRootAction(organism);
            }

            if (string.IsNullOrEmpty(action))
            {
                action = CheckForSporerAction(organism);
            }

            if (string.IsNullOrEmpty(action))
            {
                action = CheckForHarvestOrBasicAction(organism);
            }

            // If there wasn't a protein to go to just spread randomly...for now
            if (string.IsNullOrEmpty(action) &&
                CostCalculator.CanProduceOrgan(OrganType.BASIC, PlayerProteinStock))
            {
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

                    Protein havestedProtein = Proteins.Single(p => p.Position == harvestedPosition);

                    havestedProtein.IsHarvested = true;
                }
            }
        }

        // We don't care about enemy harvested proteins because
        // we're still happy to consume them.
    }

    private string CheckForSporerAction(Organism organism)
    {
        string action = string.Empty;

        int minRootSporerDistance = 5;

        // TODO: We need to be able to spawn multiple sporers
        if (CostCalculator.CanProduceOrgan(OrganType.ROOT, PlayerProteinStock) &&
            CostCalculator.CanProduceOrgan(OrganType.SPORER, PlayerProteinStock))
        {
            List<Protein> unharvestedByMeProteins = Proteins.Where(p => !p.IsHarvested).ToList();

            int leastStepsToProtein = int.MaxValue;
            int quickestOrganId = -1;
            Point quickestPoint = new Point(-1, -1);
            OrganDirection quickestDirection = OrganDirection.N;

            foreach (Protein protein in unharvestedByMeProteins)
            {
                // Console.Error.WriteLine($"Checking protein ({protein.Position.X},{protein.Position.Y})");
                List<Point> possibleRootPoints = MapChecker.GetRootPoints(protein.Position, this);

                // TODO: order by closest to enemy (i.e. We want to be able to block and
                //       destroy the enemy before they can get to the protein

                // TODO: This is very intensive. Maybe add a cutoff for the 
                //       AStar search so that it doesn't keep searching
                foreach (Point rootPoint in possibleRootPoints)
                {
                    // Console.Error.WriteLine($"   Checking rootPoint ({rootPoint.X},{rootPoint.Y})");
                    // Draw a line towards the organism
                    // West
                    bool canStillMove = true;
                    int distanceFromRootPoint = 1;

                    while (canStillMove)
                    {
                        Point currentPoint = new Point(rootPoint.X - distanceFromRootPoint, rootPoint.Y);

                        // Console.Error.WriteLine($"      Checking currentPoint ({currentPoint.X},{currentPoint.Y})");
                        // If we can't grow here we've hit an obstacle. Don't check further
                        if (!MapChecker.CanGrowOn(currentPoint, this))
                        {
                            // Console.Error.WriteLine($"         Can't grow west");
                            canStillMove = false;
                            continue;
                        }

                        // This is too close to bother spawning. Carry on 
                        // checking further
                        if (distanceFromRootPoint >= minRootSporerDistance)
                        {
                            // Console.Error.WriteLine($"         Not far enough");
                            distanceFromRootPoint++;
                            continue;
                        }

                        foreach (Organ organ in organism.Organs)
                        {
                            AStar aStar = new AStar(this);
                            List<Point> path = aStar.GetShortestPath(organ.Position, currentPoint);

                            // Console.Error.WriteLine($"         ShortestPath: {path.Count}");

                            if (path.Count < leastStepsToProtein && path.Count > 0)
                            {
                                leastStepsToProtein = path.Count;
                                quickestOrganId = organ.Id;
                                quickestPoint = path[0];

                                // TODO: Calculate the direection the path is going in
                            }          
                        }

                        distanceFromRootPoint++;
                    }
                }
            }

            if (quickestOrganId != -1)
            {
                if (leastStepsToProtein < 2)
                {
                    return $"GROW {quickestOrganId} {quickestPoint.X} {quickestPoint.Y} SPORER E";
                }
                else
                {
                    return $"GROW {quickestOrganId} {quickestPoint.X} {quickestPoint.Y} BASIC";
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
            // This assumes that an organism only has one sporer. That may not always be the case
            Organ sporer = organism.Organs.Single(o => o.Type == OrganType.SPORER);

            // foreach protein
            foreach (Protein protein in Proteins)
            {
                //Console.Error.WriteLine($"Checking protein ({protein.Position.X},{protein.Position.Y})");
                List<Point> possibleRootPoints = MapChecker.GetRootPoints(protein.Position, this);
                // TODO: order by closest to enemy (i.e. We want to be able to block and
                //       destroy the enemy before they can get to the protein

                foreach (Point possibleRootPoint in possibleRootPoints)
                {
                    //Console.Error.WriteLine($"   Checking rootPoint ({possibleRootPoint.X},{possibleRootPoint.Y})");
                    Point checkPoint = new Point(possibleRootPoint.X, possibleRootPoint.Y);
                    int xDelta = 0;
                    int yDelta = 0;

                    switch (sporer.Direction)
                    {
                        case OrganDirection.N:
                            xDelta = 0;
                            yDelta = 1;
                            break;
                        case OrganDirection.E:
                            xDelta = -1;
                            yDelta = 0;
                            break;
                        case OrganDirection.S:
                            xDelta = 0;
                            yDelta = -1;
                            break;
                        case OrganDirection.W:
                            xDelta = 1;
                            yDelta = 0;
                            break;
                    }

                    bool canStillMove = true;

                    while (canStillMove)
                    {
                        // Console.Error.WriteLine($"      Checking currentPoint ({checkPoint.X},{checkPoint.Y})");

                        if (!MapChecker.CanGrowOn(checkPoint, this))
                        {
                            canStillMove = false;
                            continue;
                        }

                        // Return the first valid spore we can. It shouldn't make a difference
                        if (checkPoint == new Point(sporer.Position.X - xDelta, sporer.Position.Y - yDelta))
                        {
                            return $"SPORE {sporer.Id} {possibleRootPoint.X} {possibleRootPoint.Y}";
                        }

                        checkPoint = new Point(checkPoint.X + xDelta, checkPoint.Y + yDelta);
                    }
                }
            }
        }

        return string.Empty;
    }

    private string CheckForHarvestOrBasicAction(Organism organism)
    {
        string action = string.Empty;

        (int closestOrgan, List<Point> shortestPath) = GetShortestPathToProtein(organism, Proteins);

        if (closestOrgan != -1)
        {
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

    private (int,List<Point>) GetShortestPathToProtein(Organism organism, List<Protein> proteins)
    {
        string action = string.Empty;

        int shortest = int.MaxValue;
        int closestId = -1;
        List<Point> shortestPath = new List<Point>();

        AStar aStar = new AStar(this);
            

        // Get the closest protein to Organs
        foreach (Protein protein in proteins)
        {
            if (!protein.IsHarvested)
            {
                foreach (var organ in organism.Organs)
                {
                    List<Point> path = aStar.GetShortestPath(organ.Position, protein.Position);
                    
                    if (path.Count < shortest && path.Count != 0)
                    {
                        shortest = path.Count;
                        shortestPath = new List<Point>(path);

                        closestId = organ.Id;
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

}

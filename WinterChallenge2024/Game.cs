using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Xml.Linq;
using static System.Collections.Specialized.BitVector32;

namespace WinterChallenge2024;

internal sealed class Game
{
    internal Organism PlayerOrganism { get; private set; }
    internal Organism OpponentOrganism { get; private set; }

    internal ProteinStock PlayerProteinStock { get; private set; }
    internal ProteinStock OpponentProteinStock { get; private set; }
    
    public List<Point> Walls { get; private set; }
    public List<Protein> Proteins { get; private set; }

    private int _width;
    private int _height;

    internal Game(int width, int height)
    {
        _width = width;
        _height = height;
    }

    internal void SetPlayerProteinStock(ProteinStock playerProteins)
    {
        PlayerProteinStock = playerProteins;
    }

    internal void SetOpponentProteinStock(ProteinStock opponentProteins)
    {
        OpponentProteinStock = opponentProteins;
    }

    internal void SetPlayerOrganism(Organism playerOrganism)
    {
        PlayerOrganism = playerOrganism;
    }

    internal void SetOpponentOrganism(Organism opponentOrganism)
    {
        OpponentOrganism = opponentOrganism;
    }

    internal void SetWalls(List<Point> walls)
    {
        Walls = walls;
    }

    internal void SetProteins(List<Protein> proteins)
    {
        Proteins = proteins;
    }

    internal List<string> GetActions()
    {
        CheckForHarvestedProtein();

        string action = string.Empty;

        //if (CanProduceHarvester() && Proteins.Exists(p => p.Type == ProteinType.A && p.IsHarvested == false))
        //{
        //    List<Protein> harvestableProteins = Proteins.Where(p => p.Type == ProteinType.A && p.IsHarvested == false).ToList();

        //    (int closestOrgan, Point closestPoint) = HeadToNearestProtein(harvestableProteins);

        //    if (closestOrgan != -1)
        //    {
        //        action = $"GROW {closestOrgan} {closestPoint.X} {closestPoint.Y} HARVESTER";
        //    }
        //}

        //if (string.IsNullOrEmpty(action))
        //{
        //Display.Organism(PlayerOrganism);
        //Display.Organism(OpponentOrganism);
        Display.Proteins(Proteins);

        (int closestOrgan, List<Point> shortestPath) = GetShortestPathToProtein(Proteins);

        if (closestOrgan != -1)
        {
            if (CanProduceHarvester() && Proteins.Exists(p => p.Type == ProteinType.A && p.IsHarvested == false))
            {
                if (shortestPath.Count == 2)
                {
                    string dir = "N";

                    // Get direction to protein 
                    if (shortestPath[0].X < shortestPath[1].X)
                    {
                        dir = "E";
                    }
                    else if (shortestPath[0].X > shortestPath[1].X)
                    {
                        dir = "W";
                    }
                    else if (shortestPath[0].Y > shortestPath[1].Y)
                    {
                        dir = "S";
                    }

                    action = $"GROW {closestOrgan} {shortestPath[0].X} {shortestPath[0].Y} HARVESTER {dir}";
                }
            }

            if (string.IsNullOrEmpty(action))
            {
                action = $"GROW {closestOrgan} {shortestPath[0].X} {shortestPath[0].Y} BASIC";
            }
        }
        // }

        if (string.IsNullOrEmpty(action))
        {
            Console.Error.WriteLine("NO PROTEIN. Move randomly");
            // There was no protein to head to. Focus on expanding the 
            // organism

            // For now just be a bit random
            for (int i = PlayerOrganism.Organs.Count-1; i >= 0; i--)
            {
                Organ current = PlayerOrganism.Organs[i];

                Console.Error.WriteLine($"Checking {current.Id}");
                { }
                if (CanMoveTo(new Point(current.Position.X+1, current.Position.Y)))
                {
                    action = $"GROW {current.Id} {current.Position.X + 1} {current.Position.Y} BASIC";
                    break;
                }

                if (CanMoveTo(new Point(current.Position.X, current.Position.Y + 1)))
                {
                    action = $"GROW {current.Id} {current.Position.X} {current.Position.Y + 1} BASIC";
                    break;
                }

                if (CanMoveTo(new Point(current.Position.X, current.Position.Y - 1)))
                {
                    action = $"GROW {current.Id} {current.Position.X} {current.Position.Y - 1} BASIC";
                    break;
                }

                if (CanMoveTo(new Point(current.Position.X - 1, current.Position.Y)))
                {
                    action = $"GROW {current.Id} {current.Position.X - 1} {current.Position.Y} BASIC";
                    break;
                }
            }
        }

        if (string.IsNullOrEmpty(action))
        {
            action = "WAIT";
        }

        return new List<string>() { action };
    }

    // Check to see if any protein is being harvested and mark it as such
    private void CheckForHarvestedProtein()
    {
        foreach(Organ organ in PlayerOrganism.Organs)
        {
            if (organ.Type == OrganType.HARVESTER)
            {
                Console.Error.WriteLine("HARVESTER found");
                Point harvestedPosition = GetHarvestedPosition(organ);

                Protein havestedProtein = Proteins.Single(p => p.Position.X == harvestedPosition.X && p.Position.Y == harvestedPosition.Y);

                havestedProtein.IsHarvested = true;
            }
        }

        // We don't care about enemy harvested proteins because
        // we're still happy to consume them.
    }

    private static Point GetHarvestedPosition(Organ organ)
    {
        switch (organ.Direction)
        {
            case OrganDirection.N:
                return new Point(organ.Position.X, organ.Position.Y+1);
            case OrganDirection.E:
                return new Point(organ.Position.X+1, organ.Position.Y);
            case OrganDirection.S:
                return new Point(organ.Position.X, organ.Position.Y-1);
            case OrganDirection.W:
                return new Point(organ.Position.X-1, organ.Position.Y);
        }

        return new Point(-1,-1);
    }

    private bool CanProduceHarvester()
    {
        if (PlayerProteinStock.C >= 1 && PlayerProteinStock.D >= 1)
        {
            return true;
        }

        return false;
    }

    private (int,List<Point>) GetShortestPathToProtein(List<Protein> proteins)
    {
        string action = string.Empty;

        int shortest = int.MaxValue;
        int closestId = -1;
        List<Point> shortestPath = new List<Point>();

        AStar aStar = new AStar(this);

        // Get the closest A protein to Organs
        foreach (Protein protein in proteins)
        {
            if (protein.Type == ProteinType.A && !protein.IsHarvested)
            {
                foreach (var organ in PlayerOrganism.Organs)
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

        //Console.Error.WriteLine($"Shortest path is to organ {closestId} and is {shortestPath.Count} steps ");

        //if (closestId == -1)
        //{
        //    return (closestId, new Point(-1,-1));
        //}

        return (closestId, shortestPath);
    }

    private static double CalculateDistance(Point pointA, Point pointB)
    {
        return Math.Sqrt(Math.Pow(pointA.X - pointB.X, 2) + Math.Pow(pointA.Y - pointB.Y, 2));
    }

    private bool CanMoveTo(Point pointToCheck)
    {
        Console.Error.WriteLine($"Checking {pointToCheck.X}, {pointToCheck.Y}");
        // Not walkable if player organ on that spot
        if (PlayerOrganism.Organs.Any(o => o.Position == pointToCheck))
        {
            Console.Error.WriteLine("False at PlayerOrganism");
            return false;
        }

        // Not walkable if opponent organ on that spot
        if (OpponentOrganism.Organs.Any(o => o.Position == pointToCheck))
        {
            Console.Error.WriteLine("False at OpponentOrganism");
            return false;
        }

        // Not walkable player harvested protein on that spot
        if (Proteins.Any(p => p.IsHarvested && p.Position == pointToCheck))
        {
            Console.Error.WriteLine("False at Protein");

            Display.Proteins(Proteins);
            return false;
        }

        // Not walkable if wall on that spot
        if (Walls.Any(w => w == pointToCheck))
        {
            Console.Error.WriteLine("False at Wall");

            return false;
        }

        Console.Error.WriteLine("True");


        return true;
    }
}

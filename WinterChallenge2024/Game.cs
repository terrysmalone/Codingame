using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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

        //if (CanProduceHarvester() && Proteins.Exists(p => p.Type == ProteinType.A && p.IsHarvested == false))
        //{

        //}

        Display.Organism(PlayerOrganism);
        Console.Error.WriteLine();
        Display.Proteins(Proteins);

        string action = HeadToNearestProtein();

        if (string.IsNullOrEmpty(action))
        {
            // There was no protein to head to. Focus on expanding the 
            // organism
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

    private string HeadToNearestProtein()
    {
        string action = string.Empty;

        double closest = double.MaxValue;
        int closestId = -1;
        Point closestPoint = new Point();

        // Get the closest A protein to Organs
        foreach (Protein protein in Proteins)
        {
            if (protein.Type == ProteinType.A && !protein.IsHarvested)
            {
                foreach (var organ in PlayerOrganism.Organs)
                {
                    double distance = CalculateDistance(protein.Position, organ.Position);

                    if (distance < closest)
                    {
                        closest = distance;
                        closestId = organ.Id;
                        closestPoint = new Point(protein.Position.X, protein.Position.Y);
                    }
                }
            }
        }

        if (closestId == -1)
        {
            return string.Empty;
        }

        action = $"GROW {closestId} {closestPoint.X} {closestPoint.Y} BASIC";
        

        return action;
    }

    private static double CalculateDistance(Point pointA, Point pointB)
    {
        return Math.Sqrt(Math.Pow(pointA.X - pointB.X, 2) + Math.Pow(pointA.Y - pointB.Y, 2));
    }
}

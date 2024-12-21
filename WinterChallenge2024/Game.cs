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

        string action = string.Empty;

        (int closestOrgan, List<Point> shortestPath) = GetShortestPathToProtein(Proteins);

        if (closestOrgan != -1)
        {
            // See if we can make a harvester
            if (CanProduceHarvester() && Proteins.Exists(p => p.Type == ProteinType.A && p.IsHarvested == false))
            {
                if (shortestPath.Count == 2)
                {
                    string dir = GetDirection(shortestPath[0], shortestPath[1]);
                    
                    action = $"GROW {closestOrgan} {shortestPath[0].X} {shortestPath[0].Y} HARVESTER {dir}";
                }
            }

            // If not, just grow towards the nearest A protein
            if (string.IsNullOrEmpty(action))
            {
                action = $"GROW {closestOrgan} {shortestPath[0].X} {shortestPath[0].Y} BASIC";
            }
        }

        // If there wasn't a protein to go to just spread randomly...for now
        if (string.IsNullOrEmpty(action))
        {
            action = GetRandomBasicGrow();
        }

        if (string.IsNullOrEmpty(action))
        {
            action = "WAIT";
        }

        return new List<string>() { action };
    }

    private string GetRandomBasicGrow()
    {
        string action = string.Empty;

        for (int i = PlayerOrganism.Organs.Count - 1; i >= 0; i--)
        {
            Organ current = PlayerOrganism.Organs[i];

            if (CanMoveTo(new Point(current.Position.X + 1, current.Position.Y)))
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

        return action;
    }

    // Check to see if any protein is being harvested and mark it as such
    private void CheckForHarvestedProtein()
    {
        foreach(Organ organ in PlayerOrganism.Organs)
        {
            if (organ.Type == OrganType.HARVESTER)
            {
                Point harvestedPosition = GetHarvestedPosition(organ);
                
                Protein havestedProtein = Proteins.Single(p => p.Position == harvestedPosition);

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

    private bool CanProduceHarvester()
    {
        if (PlayerProteinStock.C >= 1 && PlayerProteinStock.D >= 1)
        {
            return true;
        }

        return false;
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

        return (closestId, shortestPath);
    }

    private bool CanMoveTo(Point pointToCheck)
    {
        // Not walkable if player organ on that spot
        if (PlayerOrganism.Organs.Any(o => o.Position == pointToCheck))
        {
            return false;
        }

        // Not walkable if opponent organ on that spot
        if (OpponentOrganism.Organs.Any(o => o.Position == pointToCheck))
        {
            return false;
        }

        // Not walkable player harvested protein on that spot
        if (Proteins.Any(p => p.IsHarvested && p.Position == pointToCheck))
        {
            return false;
        }

        // Not walkable if wall on that spot
        if (Walls.Any(w => w == pointToCheck))
        {
            return false;
        }

        return true;
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;

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
        // First pass simple solution. Find the closest A protein.
        string action =  "WAIT";

        if (PlayerOrganism.Organs.Count == 0)
        {
            double closest = double.MaxValue;
            Point closestPoint = new Point();

            // Display.Proteins(Proteins);

            // Get the closes A protein to Root
            foreach (Protein protein in Proteins)
            {
                if (protein.Type == ProteinType.A)
                {
                    double distance = CalculateDistance(protein.Position, PlayerOrganism.Root.Position);

                    if (distance < closest)
                    {
                        closest = distance;
                        closestPoint = new Point(protein.Position.X, protein.Position.Y);    
                    }
                }
            }

            action = $"GROW {PlayerOrganism.Root.Id} {closestPoint.X} {closestPoint.Y} BASIC";
        }
        else
        {
            double closest = double.MaxValue;
            int closestId = -1;
            Point closestPoint = new Point();

            // Get the closest A protein to Organs
            foreach (Protein protein in Proteins)
            {
                if (protein.Type == ProteinType.A)
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

            action = $"GROW {closestId} {closestPoint.X} {closestPoint.Y} BASIC";
        }


        return new List<string>() { action };
    }

    private static double CalculateDistance(Point pointA, Point pointB)
    {
        return Math.Sqrt(Math.Pow(pointA.X - pointB.X, 2) + Math.Pow(pointA.Y - pointB.Y, 2));
    }
}

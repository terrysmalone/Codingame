using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterChallenge2024;
internal static class MapChecker
{
    internal static int CalculateManhattanDistance(Point position1, Point position2)
    {
        return Math.Abs(position1.X - position2.X) + Math.Abs(position1.Y - position2.Y);
    }

    internal static bool CanGrowOn(Point pointToCheck, Game game)
    {
        return CanGrowOn(pointToCheck, game, GrowStrategy.NO_PROTEINS);
    }

    internal static bool CanGrowOn(Point pointToCheck, Game game, GrowStrategy growStrategy)
    {
        if (pointToCheck.X < 0 || 
            pointToCheck.Y < 0 || 
            pointToCheck.X >= game.Width || 
            pointToCheck.Y >= game.Height) 
        { 
            return false; 
        }

        if (game.isBlocked[pointToCheck.X, pointToCheck.Y])
        {
            return false;
        }

        if (growStrategy == GrowStrategy.NO_PROTEINS && game.hasAnyProtein[pointToCheck.X, pointToCheck.Y])
        {
            return false;
        }
        else if (growStrategy == GrowStrategy.UNHARVESTED && game.hasHarvestedProtein[pointToCheck.X, pointToCheck.Y])
        {
            return false;
        }

        return true;
    }

    internal static List<Point> GetRootPoints(Point position, Game game)
    {
        List<Point> rootPoints = new List<Point>();

        bool canGrowNorth = CanGrowOn(new Point(position.X, position.Y - 1), game);
        bool canGrowEast = CanGrowOn(new Point(position.X+1, position.Y), game);
        bool canGrowSouth = CanGrowOn(new Point(position.X, position.Y + 1), game);
        bool canGrowWest = CanGrowOn(new Point(position.X-1, position.Y), game);

        if (canGrowNorth)
        {
            Point farNorth = new Point(position.X, position.Y - 2);
            if (CanGrowOn(farNorth, game))
            {
                rootPoints.Add(farNorth);
            }
        }

        if (canGrowNorth || canGrowEast)
        {
            Point northEast = new Point(position.X + 1, position.Y - 1);
            if (CanGrowOn(northEast, game))
            {
                rootPoints.Add(northEast);
            }
        }

        if (canGrowEast)
        {
            Point farEast = new Point(position.X + 2, position.Y);
            if (CanGrowOn(farEast, game))
            {
                rootPoints.Add(farEast);
            }
        }

        if (canGrowEast ||canGrowSouth)
        {
            Point southEast = new Point(position.X + 1, position.Y + 1);
            if (CanGrowOn(southEast, game))
            {
                rootPoints.Add(southEast);
            }
        }

        if (canGrowSouth)
        {
            Point farSouth = new Point(position.X, position.Y + 2);
            if (CanGrowOn(farSouth, game))
            {
                rootPoints.Add(farSouth);
            }
        }

        if (canGrowSouth || canGrowWest)
        {
            Point southWest = new Point(position.X - 1, position.Y + 1);
            if (CanGrowOn(southWest, game))
            {
                rootPoints.Add(southWest);
            }
        }

        if (canGrowWest)
        {
            Point farWest = new Point(position.X - 2, position.Y);
            if (CanGrowOn(farWest, game))
            {
                rootPoints.Add(farWest);
            }
        }

        if (canGrowWest || canGrowNorth)
        {
            Point northWest = new Point(position.X - 1, position.Y - 1);
            if (CanGrowOn(northWest, game))
            {
                rootPoints.Add(northWest);
            }
        }

        return rootPoints;
    }

    internal static bool HasNearbyOrgan(Protein protein, List<Organism> playerOrganisms)
    {
        int maxDistance = 3;
        foreach (Organism organism in playerOrganisms)
        {
            foreach (Organ organ in organism.Organs)
            {
                if (Math.Abs(protein.Position.X - organ.Position.X) + Math.Abs(protein.Position.Y - organ.Position.Y) <= maxDistance)
                {
                    return true;
                }
            }
        }

        return false;
    }

    // If we can draw a line from the sporer to a root then it's spored
    internal static bool HasSporerSpored(Organ sporer, Game game)
    {
        Console.Error.WriteLine($"Checking if sporer on {sporer.Position.X},{sporer.Position.Y} has spored");
        int xDelta = 0;
        int yDelta = 0;

        switch(sporer.Direction)
        {
            case OrganDirection.N:
                xDelta = 0;
                yDelta = -1;
                break;
            case OrganDirection.E:
                xDelta = 1;
                yDelta = 0;
                break;
            case OrganDirection.S:
                xDelta = 0;
                yDelta = 1;
                break;
            case OrganDirection.W:
                xDelta = -1;
                yDelta = 0;
                break;
        }

        bool hitSomething = false;

        Point checkPoint = new Point(sporer.Position.X + xDelta, sporer.Position.Y + yDelta);

        while(!hitSomething)
        {
            foreach (Organism organism in game.PlayerOrganisms)
            {
                if(organism.Organs.Any(o => o.Type == OrganType.ROOT &&
                                            o.Position == checkPoint))
                {
                    return true;
                }
            }

            if (!CanGrowOn(checkPoint, game, GrowStrategy.UNHARVESTED))
            {
                hitSomething = true;    

            }

            checkPoint = new Point(checkPoint.X + xDelta, checkPoint.Y + yDelta);
        }

        return false;
    }
}

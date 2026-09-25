using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace WinterChallenge2024;
internal static class MapChecker
{
    internal static int CalculateManhattanDistance(Point position1, Point position2)
    {
        return Math.Abs(position1.X - position2.X) + Math.Abs(position1.Y - position2.Y);
    }

    internal static bool CanGrowOn(Point pointToCheck, Map map)
    {
        return CanGrowOn(pointToCheck, map, GrowStrategy.NO_PROTEINS, false);
    }

    internal static bool CanGrowOn(Point pointToCheck, Map map, GrowStrategy growStrategy, bool walkAcrossEnemyTentacles)
    {
        if (pointToCheck.X < 0 || 
            pointToCheck.Y < 0 || 
            pointToCheck.X >= map.Width || 
            pointToCheck.Y >= map.Height) 
        { 
            return false; 
        }

        if (map.IsBlocked(pointToCheck.X, pointToCheck.Y))
        {
            return false;
        }

        if (!walkAcrossEnemyTentacles)
        {
            if (map.HasOpponentTentaclePath(pointToCheck.X, pointToCheck.Y))
            {
                return false;
            }
            
        }

        if (growStrategy == GrowStrategy.NO_PROTEINS && map.HasAnyProtein(pointToCheck.X, pointToCheck.Y))
        {
            return false;
        }
        else if (growStrategy == GrowStrategy.UNHARVESTED && map.HasHarvestedProtein(pointToCheck.X, pointToCheck.Y))
        {
            return false;
        }

        return true;
    }

    internal static List<Point> GetRootPoints(Point position, Map map)
    {
        List<Point> rootPoints = new List<Point>();

        bool canGrowNorth = CanGrowOn(new Point(position.X, position.Y - 1), map);
        bool canGrowEast = CanGrowOn(new Point(position.X+1, position.Y), map);
        bool canGrowSouth = CanGrowOn(new Point(position.X, position.Y + 1), map);
        bool canGrowWest = CanGrowOn(new Point(position.X-1, position.Y), map);

        if (canGrowNorth)
        {
            Point farNorth = new Point(position.X, position.Y - 2);
            if (CanGrowOn(farNorth, map))
            {
                rootPoints.Add(farNorth);
            }
        }

        if (canGrowNorth || canGrowEast)
        {
            Point northEast = new Point(position.X + 1, position.Y - 1);
            if (CanGrowOn(northEast, map))
            {
                rootPoints.Add(northEast);
            }
        }

        if (canGrowEast)
        {
            Point farEast = new Point(position.X + 2, position.Y);
            if (CanGrowOn(farEast, map))
            {
                rootPoints.Add(farEast);
            }
        }

        if (canGrowEast ||canGrowSouth)
        {
            Point southEast = new Point(position.X + 1, position.Y + 1);
            if (CanGrowOn(southEast, map))
            {
                rootPoints.Add(southEast);
            }
        }

        if (canGrowSouth)
        {
            Point farSouth = new Point(position.X, position.Y + 2);
            if (CanGrowOn(farSouth, map))
            {
                rootPoints.Add(farSouth);
            }
        }

        if (canGrowSouth || canGrowWest)
        {
            Point southWest = new Point(position.X - 1, position.Y + 1);
            if (CanGrowOn(southWest, map))
            {
                rootPoints.Add(southWest);
            }
        }

        if (canGrowWest)
        {
            Point farWest = new Point(position.X - 2, position.Y);
            if (CanGrowOn(farWest, map))
            {
                rootPoints.Add(farWest);
            }
        }

        if (canGrowWest || canGrowNorth)
        {
            Point northWest = new Point(position.X - 1, position.Y - 1);
            if (CanGrowOn(northWest, map))
            {
                rootPoints.Add(northWest);
            }
        }

        return rootPoints;
    }

    internal static bool HasNearbyOrgan(Point point, List<Organism> playerOrganisms, int minDistance)
    {
        foreach (Organism organism in playerOrganisms)
        {
            foreach (Organ organ in organism.Organs)
            {
                if (Math.Abs(point.X - organ.Position.X) + Math.Abs(point.Y - organ.Position.Y) <= minDistance)
                {
                    return true;
                }
            }
        }

        return false;
    }
}

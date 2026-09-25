using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace WinterChallenge2024;

internal class DirectionCalculator
{
    private readonly Map _map;

    private readonly List<Point> _directions = new List<Point>
    {
        new Point(0, 1),
        new Point(0, -1),
        new Point(1, 0),
        new Point(-1, 0)
    };

    public DirectionCalculator(Map map)
    {
        _map = map;
    }


    internal OrganDirection? CalculateClosestOpponentDirection(Point startPoint, List<Organism> opponentOrganisms)
    {
        Point endPoint = GetClosestRoot(startPoint, opponentOrganisms);

        return CalculateClosestOpponentDirection(startPoint, endPoint);
    }
    internal OrganDirection? CalculateClosestOpponentDirection(Point startPoint, Point endPoint)
    {
        if (Math.Abs(endPoint.X - startPoint.X) >= Math.Abs(endPoint.Y - startPoint.Y))
        {
            // It's either east or west
            if (endPoint.X > startPoint.X)
            {
                if (startPoint.X + 1 < _map.Width && !_map.HasWall(startPoint.X + 1, startPoint.Y))
                {
                    return OrganDirection.E;
                }
            }
            else
            {
                if (startPoint.X - 1 >= 0 && !_map.HasWall(startPoint.X - 1, startPoint.Y))
                {
                    return OrganDirection.W;
                }
            }
        }
        else
        {
            // It's either north or south
            if (endPoint.Y > startPoint.Y)
            {
                if (startPoint.Y + 1 < _map.Height && !_map.HasWall(startPoint.X, startPoint.Y + 1))
                {
                    return OrganDirection.S;
                }
            }
            else
            {
                if (startPoint.Y - 1 >= 0 && !_map.HasWall(startPoint.X, startPoint.Y - 1))
                {
                    return OrganDirection.N;
                }
            }
        }

        // If we've gotten this far it means that pointing towards the 
        // opponents main root would point towards a wall. We don't want that. 
        // Grow towards an open space
        foreach (Point direction in _directions)
        {
            Point directionPoint = new Point(startPoint.X + direction.X, startPoint.Y + direction.Y);



            if (MapChecker.CanGrowOn(
                directionPoint,
                _map,
                GrowStrategy.ALL_PROTEINS,
                false))
            {
                return GetDirection(startPoint, directionPoint);
            }
        }

        // if we got this far it really doesn't matter 
        return OrganDirection.E;
    }

    internal OrganDirection? GetDirection(Point from, Point to)
    {
        OrganDirection dir = OrganDirection.N;

        if (from.X < to.X)
        {
            dir = OrganDirection.E;
        }
        else if (from.X > to.X)
        {
            dir = OrganDirection.W;
        }
        else if (from.Y < to.Y)
        {
            dir = OrganDirection.S;
        }

        return dir;
    }

    internal Point GetDelta(OrganDirection organDirection)
    {
        switch (organDirection)
        {
            case OrganDirection.N:
                return new Point(0, -1);
            case OrganDirection.E:
                return new Point(1, 0);
            case OrganDirection.S:
                return new Point(0, 1);
            case OrganDirection.W:
                return new Point(-1, 0);
            default:
                return new Point(0, 0);
        }
    }

    private static Point GetClosestRoot(Point startPoint, List<Organism> opponentOrganisms)
    {
        int closestDistance = int.MaxValue;
        Point closestPoint = new Point(-1, -1);

        foreach (Organism opponentOrganism in opponentOrganisms)
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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace WinterChallenge2024;

internal class DirectionCalculator
{
    private readonly Game _game;

    private readonly List<Point> _directions = new List<Point>
    {
        new Point(0, 1),
        new Point(0, -1),
        new Point(1, 0),
        new Point(-1, 0)
    };

    public DirectionCalculator(Game game)
    {
        _game = game;
    }


    internal OrganDirection? CalculateClosestOpponentDirection(Point startPoint)
    {
        Point endPoint = GetClosestRoot(startPoint);

        return CalculateClosestOpponentDirection(startPoint, endPoint);
    }
    internal OrganDirection? CalculateClosestOpponentDirection(Point startPoint, Point endPoint)
    {
        if (Math.Abs(endPoint.X - startPoint.X) >= Math.Abs(endPoint.Y - startPoint.Y))
        {
            // It's either east or west
            if (endPoint.X > startPoint.X)
            {
                if (startPoint.X + 1 < _game.Width && !_game.Walls[startPoint.X + 1, startPoint.Y])
                {
                    return OrganDirection.E;
                }
            }
            else
            {
                if (startPoint.X - 1 >= 0 && !_game.Walls[startPoint.X - 1, startPoint.Y])
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
                if (startPoint.Y + 1 < _game.Height && !_game.Walls[startPoint.X, startPoint.Y + 1])
                {
                    return OrganDirection.S;
                }
            }
            else
            {
                if (startPoint.Y - 1 >= 0 && !_game.Walls[startPoint.X, startPoint.Y - 1])
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
                _game,
                GrowStrategy.ALL_PROTEINS))
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

    private Point GetClosestRoot(Point startPoint)
    {
        int closestDistance = int.MaxValue;
        Point closestPoint = new Point(-1, -1);

        foreach (Organism opponentOrganism in _game.OpponentOrganisms)
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

using System;
using System.Collections.Generic;
using System.Drawing;

namespace _2023_2_FallChallenge_SeabedSecurity;

internal static class DistanceCalculator
{
    internal static int GetDistance(Point position1, Point position2)
    {
        var dx = position1.X - position2.X;
        var dy = position1.Y - position2.Y;
        return (int)Math.Sqrt(dx * dx + dy * dy);
    }

    internal static Point GetPointAlongPath(Point position, Point target, int droneSpeed)
    {
        var dx = target.X - position.X;
        var dy = target.Y - position.Y;
        var distance = Math.Sqrt(dx * dx + dy * dy);
        if (distance == 0)
        {
            return position; // Already at the target
        }
        var ratio = droneSpeed / distance;
        var newX = (int)(position.X + dx * ratio);
        var newY = (int)(position.Y + dy * ratio);
        return new Point(newX, newY);
    }

    // Determines whether two objects, each moving linearly from start to target in one game turn,
    // will ever be within a given proximity (default 500 units) of each other during their movement.
    internal static bool WillPathsConverge(
        Point start1, Point target1,
        Point start2, Point target2,
        int proximityThreshold = 500)
    {
        // Velocity is simply (target - start) since each entity travels the full path in one turn (t in [0, 1])
        double vx1 = target1.X - start1.X;
        double vy1 = target1.Y - start1.Y;

        double vx2 = target2.X - start2.X;
        double vy2 = target2.Y - start2.Y;

        // Relative position and velocity
        double relX = start1.X - start2.X;
        double relY = start1.Y - start2.Y;
        double relVx = vx1 - vx2;
        double relVy = vy1 - vy2;

        // Squared distance: f(t) = (relX + t*relVx)^2 + (relY + t*relVy)^2
        //                        = a*t^2 + b*t + c
        double a = relVx * relVx + relVy * relVy;
        double b = 2.0 * (relX * relVx + relY * relVy);
        double c = relX * relX + relY * relY;

        double thresholdSq = (double)proximityThreshold * proximityThreshold;

        // Check start (t=0)
        if (c <= thresholdSq)
        {
            return true;
        }

        // Check end (t=1)
        double distSqAtEnd = a + b + c;
        if (distSqAtEnd <= thresholdSq)
        {
            return true;
        }

        // Check the minimum of the quadratic within [0, 1]
        if (a > 0)
        {
            double tMin = -b / (2.0 * a);

            if (tMin > 0 && tMin < 1)
            {
                double minDistSq = a * tMin * tMin + b * tMin + c;
                if (minDistSq <= thresholdSq)
                {
                    return true;
                }
            }
        }

        return false;
    }

}
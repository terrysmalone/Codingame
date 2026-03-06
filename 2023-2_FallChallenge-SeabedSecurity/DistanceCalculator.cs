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

    // Determines whether two objects, each moving along a linear path at their own speed,
    // will ever be within a given proximity (default 500 units) of each other during their movement.
    // Returns true if at any point during one step of movement the two entities are within proximityThreshold units.
    internal static bool WillPathsConverge(
        Point start1, Point target1, int speed1,
        Point start2, Point target2, int speed2,
        int proximityThreshold = 500)
    {
        // Calculate direction vectors scaled to each entity's speed
        double dx1 = target1.X - start1.X;
        double dy1 = target1.Y - start1.Y;
        double dist1 = Math.Sqrt(dx1 * dx1 + dy1 * dy1);

        double dx2 = target2.X - start2.X;
        double dy2 = target2.Y - start2.Y;
        double dist2 = Math.Sqrt(dx2 * dx2 + dy2 * dy2);

        // Velocity vectors (units moved per step, i.e. over t in [0, 1])
        double vx1 = dist1 > 0 ? (dx1 / dist1) * speed1 : 0;
        double vy1 = dist1 > 0 ? (dy1 / dist1) * speed1 : 0;

        double vx2 = dist2 > 0 ? (dx2 / dist2) * speed2 : 0;
        double vy2 = dist2 > 0 ? (dy2 / dist2) * speed2 : 0;

        // Position at time t (0 <= t <= 1):
        //   P1(t) = start1 + t * v1
        //   P2(t) = start2 + t * v2
        //
        // Difference: D(t) = (start1 - start2) + t * (v1 - v2)
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

        // If paths are not parallel/same velocity, check the minimum of the quadratic
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
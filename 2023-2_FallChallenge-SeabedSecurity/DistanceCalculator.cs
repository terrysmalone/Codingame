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


}
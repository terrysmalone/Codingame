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
}
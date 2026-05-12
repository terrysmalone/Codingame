using System;
using System.Drawing;

namespace SpringChallenge2026;

internal static class CalculationUtil
{
    internal static int GetManhattanDistance(Point point1, Point point2)
    {
        return Math.Abs(point1.X - point2.X) + Math.Abs(point1.Y - point2.Y);
    }
}



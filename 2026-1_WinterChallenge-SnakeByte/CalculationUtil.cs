using System.Drawing;

namespace _2026_1_WinterChallenge_SnakeByte;

internal static class CalculationUtil
{
    internal static int GetManhattanDistance(Point point1, Point point2)
    {
        return Math.Abs(point1.X - point2.X) + Math.Abs(point1.Y - point2.Y);

    }
}

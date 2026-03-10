using System.Drawing;

namespace _2026_1_WinterChallenge_SnakeByte;

internal static class DirectionHelper
{
    internal static string GetDirection(Point point1, Point point2)
    {
        if (point1.X < point2.X)
        {
            return "RIGHT";
        }
        else if (point1.X > point2.X)
        {
            return "LEFT";
        }
        else if (point1.Y < point2.Y)
        {
            return "DOWN";
        }
        else if (point1.Y > point2.Y)
        {
            return "UP";
        }
        else
        {
            throw new Exception($"Unable to determine direction from {point1} to {point2}");
        }
    }
}
using System;
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
            Console.Error.WriteLine($"ERROR: Unable to determine direction from {point1} to {point2}");
            return "ERROR";
        }
    }

    internal static Point GetNewPosition(Point point, string direction)
    {
        return direction switch
        {
            "UP" => new Point(point.X, point.Y - 1),
            "DOWN" => new Point(point.X, point.Y + 1),
            "LEFT" => new Point(point.X - 1, point.Y),
            "RIGHT" => new Point(point.X + 1, point.Y),
            _ => throw new Exception($"Unable to determine new position from {point} and direction {direction}")
        };
    }
}
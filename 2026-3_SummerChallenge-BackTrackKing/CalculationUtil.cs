using System;
using System.Drawing;

namespace BackTrackKing;

internal class CalculationUtil
{
    internal static int GetManhattanDistance(Point startPosition, Point targetPosition)
    {
        return Math.Abs(startPosition.X - targetPosition.X) + Math.Abs(startPosition.Y - targetPosition.Y);
    }
}
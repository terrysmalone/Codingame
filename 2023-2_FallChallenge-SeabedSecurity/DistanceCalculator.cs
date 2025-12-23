using System;
using System.Collections.Generic;
using System.Drawing;

namespace _2023_2_FallChallenge_SeabedSecurity;

internal class DistanceCalculator
{
    private List<Creature> creatures;
    public DistanceCalculator(List<Creature> allCreatures)
    {
        creatures = allCreatures;
    }

    internal Point GetClosestCreaturePosition(Drone drone, bool unscannedByMe, bool unscannedByEnemy)
    {
        return new Point(0, 0);
    }

    private static int GetDistance(Point position1, Point position2)
    {
        var dx = position1.X - position2.X;
        var dy = position1.Y - position2.Y;
        return (int)Math.Sqrt(dx * dx + dy * dy);
    }
}
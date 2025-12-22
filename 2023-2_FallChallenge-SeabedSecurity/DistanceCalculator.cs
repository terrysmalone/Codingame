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
        var closest = int.MaxValue;
        var closestId = -1;
        Point closestPosition = new Point(0, 0);

        foreach (var creature in creatures)
        {
            var dist = GetDistance(drone.Position, creature.Position);

            if(unscannedByMe && creature.IsScannedByMe)
            {
                continue;
            }

            if(unscannedByEnemy && creature.IsScannedByEnemy)
            {
                continue;
            }

            if (dist < closest)
            {
                closest = dist;
                closestId = creature.Id;
                closestPosition = creature.Position;
            }
        }

        return closestPosition;
    }

    private static int GetDistance(Point position1, Point position2)
    {
        var dx = position1.X - position2.X;
        var dy = position1.Y - position2.Y;
        return (int)Math.Sqrt(dx * dx + dy * dy);
    }
}
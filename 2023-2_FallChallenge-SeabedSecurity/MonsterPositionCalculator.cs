using System;
using System.Drawing;

namespace _2023_2_FallChallenge_SeabedSecurity;

// Monster rules on position
// When are they visible?
// Monsters are detectable 300 units beyond your light radius (Light radius is 800 by default and 2000)
// If within light radius the monster will chase at 540 per turn.
internal sealed class MonsterPositionCalculator
{
    private readonly Game _game;
    public MonsterPositionCalculator(Game game)
    {
        _game = game;
    }

    internal Point PredictTargetPosition(Creature monster)
    {
        Console.Error.WriteLine($"Predicting position for monster {monster.Id}");
        // Get nearest drone
        // If nearest drone is using battery light is 2000, otherwise 800
        // If creature is within light radius, it will chase the nearest drone at 540 per turn
        var nearestDrone = _game.GetNearestDrone(monster);
        var lightRadius = nearestDrone.BatteryLevel == 1 ? 2000 : 800;

        var distanceToDrone = DistanceCalculator.GetDistance(monster.Position, nearestDrone.Position);
                
        if (distanceToDrone <= lightRadius)
        {
            // Monster will chase the nearest drone
            var direction = new Point(nearestDrone.Position.X - monster.Position.X, nearestDrone.Position.Y - monster.Position.Y);        
            var magnitude = Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y);
            
            double normalisedX = direction.X / magnitude;
            double normalisedY = direction.Y / magnitude;

            Point targetPoint = new Point((int)(monster.Position.X + normalisedX * 540), (int)(monster.Position.Y + normalisedY * 540));

            return targetPoint;
        }
        else
        {
            // Monster will continue in the same direction
            // TOOD: Not always....
            return new Point(monster.Position.X + monster.Velocity.X, monster.Position.Y + monster.Velocity.Y);
        }
    }
}



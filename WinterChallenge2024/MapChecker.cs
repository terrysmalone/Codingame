using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterChallenge2024;
internal static class MapChecker
{
    internal static bool CanGrowOn(Point pointToCheck, Game game)
    {
        if (pointToCheck.X < 0 || 
            pointToCheck.Y < 0 || 
            pointToCheck.X >= game.Width || 
            pointToCheck.Y >= game.Height) 
        { 
            return false; 
        }
        // Not walkable if player organ on that spot
        foreach (Organism organism in game.PlayerOrganisms)
        {
            if (organism.Organs.Any(o => o.Position == pointToCheck))
            {
                return false;
            }
        }

        // Not walkable if opponent organ on that spot
        foreach (Organism organism in game.OpponentOrganisms)
        {
            if (organism.Organs.Any(o => o.Position == pointToCheck))
            {
                return false;
            }
        }

        // Not walkable player harvested protein on that spot
        if (game.Proteins.Any(p => p.IsHarvested && p.Position == pointToCheck))
        {
            return false;
        }

        // Not walkable if wall on that spot
        if (game.Walls.Any(w => w == pointToCheck))
        {
            return false;
        }

        return true;
    }

    internal static List<Point> GetRootPoints(Point position, Game game)
    {
        List<Point> rootPoints = new List<Point>();

        Point[] pointsToCheck = new Point[] {
                new Point(position.X - 1, position.Y - 1),
                new Point(position.X - 1, position.Y + 1),
                new Point(position.X + 1, position.Y - 1),
                new Point(position.X + 1, position.Y + 1),
                new Point(position.X - 2, position.Y),
                new Point(position.X + 2, position.Y),
                new Point(position.X, position.Y - 2),
                new Point(position.X, position.Y + 2),
            };

        foreach (Point point in pointsToCheck)
        {
            if (CanGrowOn(point, game))
            {
                rootPoints.Add(point);
            }
        }

        return rootPoints;
    }
}

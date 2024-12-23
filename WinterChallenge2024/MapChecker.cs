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
        return CanGrowOn(pointToCheck, false, game);
    }
    internal static bool CanGrowOn(Point pointToCheck, bool canGrowOnProteins, Game game)
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

        if (!canGrowOnProteins)
        {
            if (game.Proteins.Any(p => p.Position == pointToCheck))
            {
                return false;
            }
        }
        else
        {
            if (game.Proteins.Any(p => p.IsHarvested && p.Position == pointToCheck))
            {
                return false;
            }
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

        bool canGrowNorth = CanGrowOn(new Point(position.X, position.Y - 1), game);
        bool canGrowEast = CanGrowOn(new Point(position.X+1, position.Y), game);
        bool canGrowSouth = CanGrowOn(new Point(position.X, position.Y + 1), game);
        bool canGrowWest = CanGrowOn(new Point(position.X-1, position.Y), game);

        if (canGrowNorth)
        {
            Point farNorth = new Point(position.X, position.Y - 2);
            if (CanGrowOn(farNorth, game))
            {
                rootPoints.Add(farNorth);
            }
        }

        if (canGrowNorth || canGrowEast)
        {
            Point northEast = new Point(position.X + 1, position.Y - 1);
            if (CanGrowOn(northEast, game))
            {
                rootPoints.Add(northEast);
            }
        }

        if (canGrowEast)
        {
            Point farEast = new Point(position.X + 2, position.Y);
            if (CanGrowOn(farEast, game))
            {
                rootPoints.Add(farEast);
            }
        }

        if (canGrowEast ||canGrowSouth)
        {
            Point southEast = new Point(position.X + 1, position.Y + 1);
            if (CanGrowOn(southEast, game))
            {
                rootPoints.Add(southEast);
            }
        }

        if (canGrowSouth)
        {
            Point farSouth = new Point(position.X, position.Y + 2);
            if (CanGrowOn(farSouth, game))
            {
                rootPoints.Add(farSouth);
            }
        }

        if (canGrowSouth || canGrowWest)
        {
            Point southWest = new Point(position.X - 1, position.Y + 1);
            if (CanGrowOn(southWest, game))
            {
                rootPoints.Add(southWest);
            }
        }

        if (canGrowWest)
        {
            Point farWest = new Point(position.X - 2, position.Y);
            if (CanGrowOn(farWest, game))
            {
                rootPoints.Add(farWest);
            }
        }

        if (canGrowWest || canGrowNorth)
        {
            Point northWest = new Point(position.X - 1, position.Y - 1);
            if (CanGrowOn(northWest, game))
            {
                rootPoints.Add(northWest);
            }
        }

        return rootPoints;
    }
}

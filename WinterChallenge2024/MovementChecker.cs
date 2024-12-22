using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterChallenge2024;
internal static class MovementChecker
{
    internal static bool CanGrowOn(Point pointToCheck, Game game)
    {
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
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace _2023_2_FallChallenge_SeabedSecurity;
internal class DirectionCalculator
{
    private Game game;

    public DirectionCalculator(Game game)
    {
        this.game = game;
    }

    internal CreatureDirection GetBestDirectionFromRadarBlips(Drone drone)
    {
        // First pass
        // Just go in the direction with the most creatures
        Dictionary<CreatureDirection, int> directionCounts = new Dictionary<CreatureDirection, int>();

        foreach (var direction in drone.CreatureDirections)
        {
            var incrementAmount = 0;

            // If a creature has been scanned/stored by me don't count it
            if (game.MyScannedCreatureIds.Contains(direction.Key) || drone.ScannedCreaturesIds.Contains(direction.Key))
            {
                incrementAmount = 0;
            }
            // If it's been saved but not scanned/stored by me count it as one
            else if (game.EnemyScannedCreatureIds.Contains(direction.Key) && !game.MyScannedCreatureIds.Contains(direction.Key) && !drone.ScannedCreaturesIds.Contains(direction.Key))
            {
                incrementAmount = 1;
            }
            // If it's not been save by anyone and not stored by me count it as 3
            else if (!game.EnemyScannedCreatureIds.Contains(direction.Key) && !game.MyScannedCreatureIds.Contains(direction.Key) && !drone.ScannedCreaturesIds.Contains(direction.Key))
            {
                incrementAmount = 3;
            }

            if (directionCounts.ContainsKey(direction.Value))
            {
                directionCounts[direction.Value] += incrementAmount;
            }
            else
            {
                directionCounts[direction.Value] = incrementAmount;
            }
        }

        // return the key with the highest value
        return directionCounts.MaxBy(dc => dc.Value).Key;
         
    }
}
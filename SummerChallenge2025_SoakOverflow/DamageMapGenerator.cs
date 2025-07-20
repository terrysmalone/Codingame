
using System.Drawing;

namespace SummerChallenge2025_SoakOverflow;

// Calculates the max damage an agent can deal if positioned at any point in the map,
// Based on current opponent positions
internal class DamageMapGenerator
{
    const double SPLASH_CUTOFF = 60.0; // Minimum splash damage to consider
    private int width;
    private int height;

    public DamageMapGenerator(int width, int height)
    {
        this.width = width;
        this.height = height;
    }

    internal double[,] CreateDamageMap(Agent agent,
                                       List<Agent> opponentAgents, 
                                       int[,] splashMap, 
                                       Dictionary<int, double[,]> coverMaps,
                                       int[,] cover)
    {
        double[,] damageMap = new double[width, height];

        int maxRange = agent.OptimalRange * 2;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (cover[x, y] > 0)
                {
                    // If there is cover on this spot we can't shoot from here
                    damageMap[x, y] = 0.0;
                    continue;
                }

                var maxDamage = 0.0;

                if (agent.SplashBombs > 0)
                {
                    maxDamage = GetBestBombThrow(x, y, splashMap);
                }

                // Only check for max shoot damage if we haven't already added bomb damage
                if (maxDamage <= 0.0) 
                { 
                    foreach (var opponentAgent in opponentAgents)
                    {
                        // Check if it's within max range
                        if (CalculationUtil.GetManhattanDistance(opponentAgent.Position, new Point(x, y)) <= maxRange)
                        {
                            double damage = agent.SoakingPower / 2;

                            if (CalculationUtil.GetManhattanDistance(opponentAgent.Position, new Point(x, y)) <= agent.OptimalRange)
                            {
                                damage = agent.SoakingPower;
                            }

                            // Deduct points for cover
                            double[,]? opponentCoverMap = coverMaps.GetValueOrDefault(opponentAgent.Id);

                            if (opponentCoverMap == null)
                            {
                                Console.Error.WriteLine($"ERROR: No cover map found for agent {opponentAgent.Id}");
                            }
                            else
                            {
                                var coverValue = opponentCoverMap[x, y];
                                if (coverValue < 1.0)
                                {
                                    damage *= coverValue;
                                }
                            }

                            if (damage > maxDamage)
                            {
                                maxDamage = damage;
                            }
                        }
                    }
                }

                damageMap[x, y] = maxDamage;
            }
        }

        return damageMap;
    }

    private double GetBestBombThrow(int x, int y, int[,] splashMap)
    {
        var maxDamage = 0.0;
        // Get the highest value in splashMap within 4 manhattan distance of x, y
        for (int dx = -4; dx <= 4; dx++)
        {
            for (int dy = -4; dy <= 4; dy++)
            {
                int splashX = x + dx;
                int splashY = y + dy;

                if (splashX >= 0 && splashX < width && splashY >= 0 && splashY < height)
                {
                    if (CalculationUtil.GetManhattanDistance(new Point(splashX, splashY), new Point(x, y)) <= 4)
                    {
                        double splashValue = splashMap[splashX, splashY];
                        if (splashValue > maxDamage && splashValue >= SPLASH_CUTOFF)
                        {
                            maxDamage = splashValue;
                        }
                    }
                }
            }
        }

        return maxDamage;
    }
}


using System.Drawing;

namespace SummerChallenge2025_SoakOverflow;

internal class DamageCalculator
{
    private CoverMapGenerator _coverMapGenerator;

    internal DamageCalculator(CoverMapGenerator coverMapGenerator)
    {
        _coverMapGenerator = coverMapGenerator;
    }

    // Calculates the maximum damage an agent can deal if positioned at (x,y)
    // based on the list of opponentAgents
    internal double CalculateHighestAttackingPlayerDamage(Agent agent, int x, int y, List<Agent> opponentAgents)
    {
        var highestDamage = 0.0;
        foreach (var enemy in opponentAgents)
        {
            var damage = CalculateDamage(
                x,
                y,
                agent.OptimalRange,
                agent.SoakingPower,
                enemy.Position.X,
                enemy.Position.Y);

            if (damage > highestDamage)
            {
                highestDamage = damage;
            }

        }

        return highestDamage;
    }


    // Calculates the total damage the position (x,y) can receive from opponentAgents
    // Note: The total is theoretical and not based on whether they can currently shoot or not
    internal double CalculateReceivingDamage(int x, int y, List<Agent> opponentAgents)
    {
        var stationaryReceivingDamage = 0.0;
        foreach (var opponentAgent in opponentAgents)
        {
            stationaryReceivingDamage += CalculateDamage(
                opponentAgent.Position.X,
                opponentAgent.Position.Y,
                opponentAgent.OptimalRange,
                opponentAgent.SoakingPower,
                x,
                y);
        }

        return stationaryReceivingDamage;
    }

    internal double CalculateDamage(int fromX, int fromY, int optimalRange, int soakingPower, int targetX, int targetY)
    {
        double[,] map = _coverMapGenerator.CreateCoverMap(targetX, targetY);

        var damageMultiplier = map[fromX, fromY];
        var baseDamage = soakingPower * damageMultiplier;

        int manhattanDistance = CalculationUtil.GetManhattanDistance(
            new Point(targetX, targetY), new Point(fromX, fromY));

        if (manhattanDistance <= optimalRange)
        {
            return baseDamage;
        }
        else if (manhattanDistance <= optimalRange * 2)
        {
            return baseDamage / 2;
        }
        else
        {
            return 0;
        }
    }


}

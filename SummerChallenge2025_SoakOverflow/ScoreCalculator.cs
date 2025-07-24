
using System.ComponentModel;
using System.Drawing;

namespace SummerChallenge2025_SoakOverflow;

internal class ScoreCalculator
{
    private int _width;
    private int _height;

    public ScoreCalculator(int width, int height)
    {
        _width = width;
        _height = height;
    }

    internal (int player, int opponent) CalculateScores(List<Agent> playerAgents, List<Agent> opponentAgents)
    {
        var player = 0;
        var opponent = 0;

        for (var y = 0; y < _height; y++)
        {
            for (var x = 0; x < _width; x++)
            {
                int closestPlayerDistance = GetClosestAgentDistance(playerAgents, x, y);
                int closestOpponentDistance = GetClosestAgentDistance(opponentAgents, x, y);

                if (closestPlayerDistance < closestOpponentDistance)
                {
                    player++;
                }
                else if (closestPlayerDistance > closestOpponentDistance)
                {
                    opponent++;
                }
            }
        }

        return (player, opponent);
    }

    private int GetClosestAgentDistance(List<Agent> agents, int x, int y)
    {
        var closest = int.MaxValue;
        foreach (var agent in agents)
        {
            var distance = CalculationUtil.GetManhattanDistance(agent.Position, new Point(x, y));

            if (agent.Wetness >= 50)
            {
                distance *= 2;
            }

            if (distance < closest)
            {
                closest = distance;
            }
        }

        return closest;
    }
}
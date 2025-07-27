
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

    internal int CalculateScoreDiff(List<Agent> playerAgents, List<Agent> opponentAgents)
    {
        (var player, var opponent) = CalculateScores(playerAgents, opponentAgents);
        return player - opponent;
    }

    internal (int player, int opponent) CalculateScores(List<Agent> playerAgents, List<Agent> opponentAgents)
    {
        List<(Point, int)> players = playerAgents.Select(a => (a.Position, a.Wetness)).ToList();
        List<(Point, int)> opponents = opponentAgents.Select(a => (a.Position, a.Wetness)).ToList();

        return CalculateScores(players, opponents);
    }

    internal (int player, int opponent) CalculateScores(List<Agent> playerAgents, Dictionary<int, Point> playerChanges, List<Agent> opponentAgents)
    {
        List<(Point, int)> players = new List<(Point, int)>();
        foreach (var agent in playerAgents)
        {
            if (playerChanges.TryGetValue(agent.Id, out var newPosition))
            {
                players.Add((newPosition, agent.Wetness));
            }
            else
            {
                players.Add((agent.Position, agent.Wetness));
            }
        }

        List<(Point, int)> opponents = opponentAgents.Select(a => (a.Position, a.Wetness)).ToList();

        return CalculateScores(players, opponents);
    }

    internal (int player, int opponent) CalculateScores(List<(Point, int)> playerAgents, List<(Point, int)> opponentAgents)
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

    private int GetClosestAgentDistance(List<(Point, int)> agents, int x, int y)
    {
        var closest = int.MaxValue;
        foreach (var agent in agents)
        {
            var distance = CalculationUtil.GetManhattanDistance(agent.Item1, new Point(x, y));

            if (agent.Item2 >= 50)
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
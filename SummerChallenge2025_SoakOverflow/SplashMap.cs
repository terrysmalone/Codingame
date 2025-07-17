


namespace SummerChallenge2025_SoakOverflow;

internal class SplashMap
{
    private int width;
    private int height;
    private List<Agent> playerAgents;
    private List<Agent> opponentAgents;

    public SplashMap(int width, int height, List<Agent> playerAgents, List<Agent> opponentAgents)
    {
        this.width = width;
        this.height = height;
        this.playerAgents = playerAgents;
        this.opponentAgents = opponentAgents;
    }

    internal int[,] CreateSplashMap()
    {
        int[,] splashMap = new int[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                splashMap[x, y] = CalculateDamage(x, y);
            }
        }

        return splashMap;
    }

    private int CalculateDamage(int x, int y)
    {
        var damage = 0;

        var minX = Math.Max(0, x - 1);
        var maxX = Math.Min(width - 1, x + 1);
        var minY = Math.Max(0, y - 1);
        var maxY = Math.Min(height - 1, y + 1);

        for (int i = minX; i <= maxX; i++)
        {
            for (int j = minY; j <= maxY; j++)
            {
                foreach (var agent in playerAgents)
                {
                    if (agent.Position.X == i && agent.Position.Y == j)
                    {
                        // If we hit an agent we want to return no damage
                        return 0;
                    }
                }

                foreach (var enemy in opponentAgents)
                {
                    if (enemy.Position.X == i && enemy.Position.Y == j)
                    {
                        damage += 30;
                    }
                }
            }
        }

        return damage;
    }
}
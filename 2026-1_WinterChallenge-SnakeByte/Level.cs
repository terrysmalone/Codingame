using System.Drawing;

namespace _2026_1_WinterChallenge_SnakeByte;

internal class Level
{
    private int width;
    private int height;

    internal bool[,] Platforms { get; private set; }

    internal List<Point> PowerSources { get; private set; } = new List<Point>();

    public Level(int width, int height, bool[,] platforms)
    {
        this.width = width;
        this.height = height;

        Platforms = platforms;
    }

    internal bool IsPlatform(Point pointToCheck)
    {
        return Platforms[pointToCheck.Y, pointToCheck.X];
    }

    internal HashSet<Point> GetAllPlatformPositions()
    {
        var platformPositions = new HashSet<Point>();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (Platforms[y, x])
                {
                    platformPositions.Add(new Point(x, y));
                }
            }
        }

        return platformPositions;
    }
}

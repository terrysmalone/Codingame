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
        // We allow snakes to move off the board
        if (pointToCheck.X < 0 || pointToCheck.X >= width || pointToCheck.Y < 0 || pointToCheck.Y >= height)
        {
            return false;
        }

        return Platforms[pointToCheck.Y, pointToCheck.X];
    }
}

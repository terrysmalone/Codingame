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
}

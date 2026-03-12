using System.Drawing;

namespace _2026_1_WinterChallenge_SnakeByte;

internal record Plan
{
    private readonly List<Point> _moves;
    private readonly int _score;

    public Plan(List<Point> moves, int score)
    {
        _moves = moves;
        _score = score;
    }
}


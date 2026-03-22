using System.Collections.Generic;
using System.Drawing;

namespace _2026_1_WinterChallenge_SnakeByte;

internal sealed class MinimaxResult
{
    internal Dictionary<int, Point> BestMoves { get; }
    internal int Score { get; }

    internal MinimaxResult(Dictionary<int, Point> bestMoves, int score)
    {
        BestMoves = bestMoves;
        Score = score;
    }
}
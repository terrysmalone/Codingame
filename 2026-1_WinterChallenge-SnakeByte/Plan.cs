using System.Collections.Generic;
using System.Drawing;

namespace _2026_1_WinterChallenge_SnakeByte;

internal record Plan
{
    internal List<Point> Moves { get; private set; }
    internal int Score { get; private set; }
    internal string PlanType { get; private set; }
    internal int TurnsToFruition { get; private set; }

    public Plan(List<Point> moves, int score, string planType, int turnsToFruition)
    {
        Moves = moves;
        Score = score;
        PlanType = planType;
        TurnsToFruition = turnsToFruition;
    }
}


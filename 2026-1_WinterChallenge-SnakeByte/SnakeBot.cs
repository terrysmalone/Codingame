using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace _2026_1_WinterChallenge_SnakeByte;

internal class SnakeBot
{
    internal int Id { get; }
    internal List<Point> Body { get; set; }
    
    internal bool Remove { get; set; }

    private List<Point> previousMoves = new List<Point>();

    internal SnakeBot(int id)
    {
        Id = id;

        Body = new List<Point>();
    }

    internal void AddMove(Point move)
    {
        previousMoves.Add(move);
    }

    internal bool IsStuck()
    {
        if (previousMoves.Count < 4)
        {
            return false;
        }

        int repetitions = 0;
        Point LastMove = previousMoves.Last();

        for (int i = previousMoves.Count - 2; i >= 0; --i)
        {
            if (previousMoves[i] == previousMoves.Last())
            {
                repetitions++;
            }
            else
            {
                break;
            }
        }

        if (repetitions > 3)
        {
            return true;
        }

        return false;
    }

    internal Point GetLastMove()
    {
        if (previousMoves.Count == 0)
        {
            return new Point(-1, -1);
        }
        return previousMoves.Last();
    }
}


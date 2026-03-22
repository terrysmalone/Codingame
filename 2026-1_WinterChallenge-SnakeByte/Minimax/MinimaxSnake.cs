using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace _2026_1_WinterChallenge_SnakeByte;

internal sealed class MinimaxSnake
{
    internal int Id { get; }
    internal List<Point> Body { get; set; }

    internal MinimaxSnake(int id, List<Point> body)
    {
        Id = id;
        Body = body.Select(p => new Point(p.X, p.Y)).ToList();
    }

    internal MinimaxSnake Clone()
    {
        return new MinimaxSnake(Id, Body);
    }
}

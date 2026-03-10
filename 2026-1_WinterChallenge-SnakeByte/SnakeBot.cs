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

    internal SnakeBot(int id)
    {
        Id = id;

        Body = new List<Point>();
    }
}


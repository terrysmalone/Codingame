using System;
using System.Text;

namespace _2026_1_WinterChallenge_SnakeByte;

internal record Move
{    
    internal int SnakeId { get; init; }
    internal string Direction { get; init; }
    internal Move(int snakeId, string direction)
    {
        SnakeId = snakeId;
        Direction = direction;
    }
}


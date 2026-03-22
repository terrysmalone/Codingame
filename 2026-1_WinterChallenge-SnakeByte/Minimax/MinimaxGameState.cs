using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace _2026_1_WinterChallenge_SnakeByte;

internal sealed class MinimaxGameState
{
    internal List<MinimaxSnake> MySnakes { get; set; } = new();
    internal List<MinimaxSnake> OpponentSnakes { get; set; } = new();
    internal HashSet<Point> PowerSources { get; set; } = new();

    internal MinimaxGameState(List<SnakeBot> mySnakes, List<SnakeBot> oppSnakes, HashSet<Point> powerSources)
    {
        MySnakes = mySnakes.Select(s => new MinimaxSnake(s.Id, s.Body)).ToList();
        OpponentSnakes = oppSnakes.Select(s => new MinimaxSnake(s.Id, s.Body)).ToList();
        PowerSources = new HashSet<Point>(powerSources);
    }

    private MinimaxGameState() { }

    internal MinimaxGameState Clone()
    {
        return new MinimaxGameState
        {
            MySnakes = MySnakes.Select(s => s.Clone()).ToList(),
            OpponentSnakes = OpponentSnakes.Select(s => s.Clone()).ToList(),
            PowerSources = new HashSet<Point>(PowerSources)
        };
    }
}

using System.Drawing;

namespace _2026_1_WinterChallenge_SnakeByte;

internal sealed class Node
{
    public Point Position { get; set; }

    public Point? Parent { get; set; }

    public int G { get; set; }
    public int H { get; set; }
    public int F { get; set; }

    public bool Closed { get; set; }

    public List<Point> SnakeBodyAtNode { get; set; } = new List<Point>();

    public Node(Point position)
    {
        Position = position;
    }
}

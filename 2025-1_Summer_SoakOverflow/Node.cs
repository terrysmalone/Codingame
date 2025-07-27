using System.Drawing;

namespace SummerChallenge2025_SoakOverflow;
internal sealed class Node
{
    public Point Position { get; set; }

    public Point Parent { get; set; }

    public int G { get; set; }
    public int H { get; set; }
    public int F { get; set; }

    public bool Closed { get; set; }

    public Node(Point position)
    {
        Position = position;
    }
}

using System.Drawing;

namespace BackTrackKing;

internal sealed class Node
{
    internal Point Position { get; set; }

    internal Node? Parent { get; set; }

    internal int G { get; set; }
    internal int H { get; set; }
    internal int F { get; set; }

    internal bool Closed { get; set; }

    // Track the move that leads to this node for tie-breaking equidistant paths
    internal Point? FirstMove { get; set; }

    internal Node(Point position)
    {
        Position = position;
    }
}


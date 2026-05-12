using System.Collections.Generic;
using System.Drawing;
using static System.Net.Mime.MediaTypeNames;

namespace SpringChallenge2026;

internal sealed class Node
{
    internal Point Position { get; set; }

    internal Node? Parent { get; set; }

    internal int G { get; set; }
    internal int H { get; set; }
    internal int F { get; set; }

    internal bool Closed { get; set; }

    internal Node(Point position)
    {
        Position = position;
    }
}

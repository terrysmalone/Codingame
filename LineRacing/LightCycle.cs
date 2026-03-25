using System.Collections.Generic;
using System.Drawing;

namespace LineRacing;

internal sealed class LightCycle
{
    internal Point StartPosition { get; set; }    
    internal Point EndPosition { get; set; }

    internal List<Point> Path { get; set; } = new List<Point>();
}
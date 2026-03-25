using System.Collections.Generic;
using System.Drawing;

namespace LineRacing;

internal sealed class LightCycle
{
    internal Point StartPosition { get; set; }    
    internal Point CurrentPosition { get; set; }

    internal List<Point> Path { get; set; } = new List<Point>();
}
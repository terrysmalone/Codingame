using System.Collections.Generic;
using System.Drawing;

namespace BackTrackKing;

internal class DesirePath
{ 
    internal List<Point> FullPath { get; private set; }

    internal List<Point> RemainingPath { get; private set; }

    internal int FullPathCount { get; set; }

    internal int FullActionCount { get; set; }

    internal int RemainingPathCount { get; set; }

    internal int RemainingActionCount { get; set; }

    public DesirePath(List<Point> fullPath, List<Point> remainingPath)
    {
        FullPath = fullPath;
        RemainingPath = remainingPath;
    }
}


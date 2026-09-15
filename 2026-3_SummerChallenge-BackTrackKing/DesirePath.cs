using System.Collections.Generic;
using System.Drawing;

namespace BackTrackKing;

internal class DesirePath
{ 
    internal List<Point> FullPath { get; private set; }

    internal List<Point> RemainingPath { get; private set; }

    internal string TownConnection { get; private set; }

    internal int FullPathCount { get; set; }

    internal int FullActionCount { get; set; }

    internal int RemainingPathCount { get; set; }

    internal int RemainingActionCount { get; set; }

    internal int MyTracksOnPathCount { get; set; }

    internal int OpponentTracksOnPathCount { get; set; }
    
    // We want to prioritise paths that have low action scores. For the untracked cells,
    // count action action cost - number of cells. Lower is better. 
    public int LowActionScore { get; internal set; }

    public DesirePath(List<Point> fullPath, List<Point> remainingPath, string townConnection)
    {
        FullPath = fullPath;
        RemainingPath = remainingPath;

        TownConnection = townConnection;
    }
}


using System.Collections.Generic;
using System.Drawing;

namespace BackTrackKing;

internal class BestPath
{
    internal int SourceTownId { get; private set; }
    internal int DestinationTownId { get; private set; }

    internal List<Point> shortestPath;

    internal BestPath(int sourceTownId, int destinationTownId)
    {
        SourceTownId = sourceTownId;
        DestinationTownId = destinationTownId;

        shortestPath = new List<Point>();
    }

    internal void SetShortestPath(List<Point> path)
    {
        shortestPath = path;
    }

    internal List<Point> GetShortestPath()
    {
        return shortestPath;
    }
}


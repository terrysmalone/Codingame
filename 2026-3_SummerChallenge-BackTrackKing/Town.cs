using System.Collections.Generic;

namespace BackTrackKing;

internal class Town
{
    internal int Id { get; private set; }

    internal int X { get; private set; }

    internal int Y { get; private set; }

    internal List<int> DesiredConnections { get; private set; }

    internal Town(int id, int x, int y, List<int> desiredConnections)
    {
        Id = id;
        X = x;
        Y = y;
        DesiredConnections = desiredConnections;
    }
}

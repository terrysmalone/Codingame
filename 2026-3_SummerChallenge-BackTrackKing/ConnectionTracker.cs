using System;
using System.Collections.Generic;

namespace BackTrackKing;

internal class ConnectionTracker
{
    private Dictionary <string, int> _connections;

    internal ConnectionTracker()
    {
        _connections = new Dictionary<string, int>();
    }

    internal void ClearConnections()
    {
        _connections.Clear();
    }
}
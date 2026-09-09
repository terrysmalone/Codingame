using System;
using System.Collections.Generic;

namespace BackTrackKing;

internal class ConnectionTracker
{
    // Connection to score map. Score is enemy tracks-mytracks.
    private Dictionary <string, int> _connectionScores;

    internal ConnectionTracker()
    {
        _connectionScores = new Dictionary<string, int>();
    }

    internal void ClearConnections()
    {
        _connectionScores.Clear();
    }

    internal void UpdateConnections(string[]? connections, bool myTrack)
    {
        if (connections == null)
        {
            return;
        }

        int addScore = myTrack ? -1 : 1;

        foreach (string connection in connections)
        {
            _connectionScores[connection] = _connectionScores.GetValueOrDefault(connection) + addScore;
        }
    }

    internal void LogConnections()
    {
        Logger.Connections(_connectionScores);
    }

    internal Dictionary<string, int> GetConnectionScores()
    {
        return _connectionScores;
    }
}
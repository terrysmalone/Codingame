using System;
using System.Collections.Generic;

namespace BackTrackKing;

internal class ConnectionTracker
{
    private int _width, _height;
    // Connection to score map. Score is enemy tracks-mytracks.
    private Dictionary <string, int> _connectionScores;

    // NOTE: This isn't currently used, but calculated each turn
    private int[,] _connectionScoresMap;

    internal ConnectionTracker(int width, int height)
    {
        _width = width;
        _height = height;

        _connectionScores = new Dictionary<string, int>();
        _connectionScoresMap = new int[width, height];
    }

    internal void ClearConnections()
    {
        _connectionScores.Clear();
        _connectionScoresMap = new int[_width, _height];
    }

    internal void UpdateConnections(string[]? connections, int x, int y, bool myTrack)
    {
        if (connections == null)
        {
            return;
        }

        int addScore = myTrack ? -1 : 1;

        foreach (string connection in connections)
        {
            _connectionScores[connection] = _connectionScores.GetValueOrDefault(connection) + addScore;
            _connectionScoresMap[x, y] += addScore;
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

    internal int[,] GetConnectionScoresMap()
    {
        return _connectionScoresMap;
    }

    internal int GetHighestAbsoluteConnectionScore()
    {
        int highestScore = 0;
        
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                int score = Math.Abs(_connectionScoresMap[x, y]);
                if (score > highestScore)
                {
                    highestScore = score;
                }
            }
        }

        return highestScore;
    }
}
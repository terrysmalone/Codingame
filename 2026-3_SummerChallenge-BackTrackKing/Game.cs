using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace BackTrackKing;

public class Game
{
    private int _myId;

    private Map _map;

    private List<Town> _towns;

    private int _myScore;
    private int _opponentScore;

    private PathFinder _pathFinder;

    public Game(int myId)
    {
        _myId = myId;

        _towns = new List<Town>();
    }

    internal void SetMap(Map map)
    {
        _map = map;

        _pathFinder = new PathFinder(_map.Width, _map.Height);
    }

    internal void SetMyScore(int myScore)
    {
        _myScore = myScore;
    }

    internal void SetOpponentScore(int foeScore)
    {
        _opponentScore = foeScore;
    }

    internal void SetTowns(List<Town> towns)
    {
        _towns = towns;
    }

    internal string CalculateActions()
    {
        // Logger.TypeMap(_map);

        // First pass
        // For all towns, for all desired paths, find the shortest path to the desired town
        foreach (var town in _towns)
        {
            Logger.Message($"Checking town {town.Id} at ({town.X}, {town.Y})");

            foreach (var desiredConnection in town.DesiredConnections)
            {
                Town desiredTown = _towns.First(t => t.Id == desiredConnection);

                Logger.Message($"Desired connection to town {desiredTown.Id} at ({desiredTown.X}, {desiredTown.Y})");
                var shortestPath = _pathFinder.GetShortestPath(new Point(town.X, town.Y), new Point(desiredTown.X, desiredTown.Y));

                Logger.Message($"Shortest path to town {desiredTown.Id} is {shortestPath.Count} steps");
            }
        }

        return "WAIT";
    }
}

using Microsoft.VisualBasic;
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

    private List<(int, int)> _completedPaths;

    public Game(int myId)
    {
        _myId = myId;

        _towns = new List<Town>();
        _completedPaths = new List<(int, int)>();
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
        // Analyse all best paths



        int actionPoints = 3; 
        // Logger.TypeMap(_map);

        // First pass
        // For all towns, for all desired paths, find the shortest path to the desired town
        List<Point> shortest = new List<Point>();
        int shortestDistance = int.MaxValue;

        foreach (var town in _towns)
        {
            foreach (var desiredConnection in town.DesiredConnections)
            {
                if (_completedPaths.Contains((town.Id, desiredConnection)))
                {
                    continue;
                }

                Town desiredTown = _towns.First(t => t.Id == desiredConnection);

                var shortestPath = _pathFinder.GetShortestPath(new Point(town.X, town.Y), new Point(desiredTown.X, desiredTown.Y));

                if (shortestPath.Count < shortestDistance)
                {
                    // Don't count the target town as part of the path
                    if (shortestPath.Count > 1)
                    {
                        shortestPath.RemoveAt(shortestPath.Count - 1);
                    }

                    // If it's 100% tracked find something else
                    if (IsAlreadyTracked(shortestPath))
                    {
                        // Add to list of completed paths so we don't try to do it again
                        _completedPaths.Add((town.Id, desiredTown.Id));
                    }
                    else
                    {
                        shortestDistance = shortestPath.Count;
                        shortest = shortestPath;
                    }
                }
            }
        }

        Logger.Message($"Shortest path found is {shortestDistance} steps with points : {string.Join(", ", shortest.Select(p => $"({p.X}, {p.Y})"))}");

        // Work out what tracks I can make
        List<(Point, CellType)> cellTypes = new List<(Point, CellType)>();

        foreach (var point in shortest)
        {
            if (_map.isTrackFree(point.X, point.Y))
            {
                CellType cellType = _map.CellTypes[point.X, point.Y];
                cellTypes.Add((point, cellType));
            }
        }

        Logger.Message($"Shortest untrakced path found is: {string.Join(", ", cellTypes.Select(c => $"({c.Item1.X}, {c.Item1.Y})"))}");

        // Order by cell type, so we can prioritize plains over rivers and mountains
        List <(Point, CellType)> orderedCellTypes = cellTypes.OrderBy(ct => ct.Item2).ToList();

        var actions = string.Empty;

        bool stop = false;
        int count = 0;
        while (actionPoints > 0 && count < orderedCellTypes.Count && stop == false)
        {
            var (point, cellType) = orderedCellTypes[count];

            if ((int)cellType + 1 <= actionPoints)
            {
                actions += $"PLACE_TRACKS {point.X} {point.Y};";
                actionPoints -= (int)cellType + 1;
            }
            else
            {
                stop = true;
            }

            count++;
        }

        return actions;
    }

    private bool IsAlreadyTracked(List<Point> path)
    {
        foreach (var point in path)
        {
            if (_map.isTrackFree(point.X, point.Y))
            {
                return false;
            }
        }

        return true;
    }

    internal void SetTrack(int j, int i, int tracksOwner)
    {
        _map.SetTrack(j, i, tracksOwner);
    }
}
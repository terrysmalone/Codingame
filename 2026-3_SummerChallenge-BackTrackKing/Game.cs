using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata.Ecma335;

namespace BackTrackKing;

public class Game
{
    private int _myId;

    private Map _map;

    private List<Town> _towns;

    private int _myScore;
    private int _opponentScore;

    private PathFinder _pathFinder;
    private RegionTracker _regionTracker;

    public Game(int myId)
    {
        _myId = myId;

        _towns = new List<Town>();

        _regionTracker = new RegionTracker(_myId);
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
        List<DesirePath> desirePaths = CalculateDesirePaths();

        // Logger.DesirePaths(desirePaths);

        var actions = CalculateActions(desirePaths);

        actions += GetDisruptAction();

        if (string.IsNullOrEmpty(actions))
        {
            actions = "WAIT;";
        }

        return actions;
    }

    private string CalculateActions(List<DesirePath> desirePaths)
    {
        string actions = string.Empty;

        int actionPoints = 3;

        foreach (var desirePath in desirePaths)
        {
            // Get all remaining tracks to place
            // Order by lowest first
            // Start allocating them

            // If we've use all 3, return

            List<(Point, CellType)> cellTypes = new List<(Point, CellType)>();

            foreach (var point in desirePath.RemainingPath)
            {
                if (_map.isTrackFree(point.X, point.Y))
                {
                    CellType cellType = _map.CellTypes[point.X, point.Y];
                    cellTypes.Add((point, cellType));
                }
            }

            // Order by cell type, so we can prioritize plains over rivers and mountains
            List<(Point, CellType)> orderedCellTypes = cellTypes.OrderBy(ct => ct.Item2).ToList();

            foreach ((Point, CellType) pair in orderedCellTypes)
            {
                if ((int)pair.Item2 + 1 <= actionPoints)
                {
                    Point cellPoint = pair.Item1;
                    int cellValue = (int)pair.Item2 + 1;
                    
                    actions += $"PLACE_TRACKS {cellPoint.X} {cellPoint.Y};";
                    actionPoints -= cellValue;
                }
                else
                {
                    continue;
                }

                if (actionPoints <= 0)
                {
                    return actions;
                }
            }
        }

        if (actionPoints > 0)
        {
            Logger.Error($"Unspent action points: {actionPoints}");
        }

        return actions;
    }

    private List<DesirePath> CalculateDesirePaths()
    {
        Logger.Message("Calculating desire paths");

        List<DesirePath> desirePaths = new List<DesirePath>();
        foreach (var town in _towns)
        {
            foreach (var desiredConnection in town.DesiredConnections)
            {
                List<Point> fullSanitisedPath = FindShortestSanitisedPath(new Point(town.X, town.Y), desiredConnection);
                
                if (fullSanitisedPath == null || fullSanitisedPath.Count == 0)
                {
                    continue;
                }

                // If it's 100% tracked continue
                if (IsAlreadyTracked(fullSanitisedPath))
                {
                    continue;
                }

                int fullPathCount = fullSanitisedPath.Count;
                int fullActionCount = CalculateActionCount(fullSanitisedPath);


                List<Point> remainingPathPoints = new List<Point>();

                foreach (var point in fullSanitisedPath)
                {
                    if (_map.isTrackFree(point.X, point.Y))
                    {
                        remainingPathPoints.Add(point);
                    }
                }

                int remainingPathCount = remainingPathPoints.Count;
                int remainingActionCount = CalculateActionCount(remainingPathPoints);

                var desirePath = new DesirePath(fullSanitisedPath, remainingPathPoints)
                {
                    FullPathCount = fullPathCount,
                    FullActionCount = fullActionCount,
                    RemainingPathCount = remainingPathCount,
                    RemainingActionCount = remainingActionCount
                };

                desirePaths.Add(desirePath);
            }
        }

        Logger.Message($"Finished calculating {desirePaths.Count} desire paths");

        return desirePaths.OrderBy(dp => dp.RemainingActionCount).ThenBy(dp => dp.RemainingPathCount).ToList();
    }

    private List<Point> FindShortestSanitisedPath(Point startPoint, int desiredConnection)
    {
        Town desiredTown = _towns.First(t => t.Id == desiredConnection);

        List<Point> shortestPath = _pathFinder.GetShortestPath(new Point(startPoint.X, startPoint.Y), new Point(desiredTown.X, desiredTown.Y), _regionTracker.GetExcludePoints());

        if (shortestPath.Count <= 0)
        {
            return new List<Point>();
        }

        // Don't count the target town as part of the path
        shortestPath.RemoveAt(shortestPath.Count - 1);

        // Don't count towns on the path
        for (int i = shortestPath.Count - 1; i >= 0; i--)
        {
            var point = shortestPath[i];
            if (_towns.Any(t => t.X == point.X && t.Y == point.Y))
            {
                shortestPath.RemoveAt(i);
                i--;
            }
        }

        return shortestPath;
    }

    private int CalculateActionCount(List<Point> path)
    {
        int actionCount = 0;

        foreach (var point in path)
        {
            CellType cellType = _map.CellTypes[point.X, point.Y];

            switch (cellType)
            {
                case CellType.PLAINS:
                    actionCount += 1;
                    break;
                case CellType.RIVER:
                    actionCount += 2;
                    break;
                case CellType.MOUNTAIN:
                    actionCount += 3;
                    break;
                default:
                    Logger.Error($"Unknown cell type: {cellType}");
                    break;
            }
        }

        return actionCount;
    }

    private string GetDisruptAction()
    {
        int region = _regionTracker.GetStrongestEnemyRegion();

        return region != -1 ? $"DISRUPT {region};" : string.Empty;
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

    internal void UpdateCell(int x, int y, int tracksOwner, int instability, bool inked)
    {
        _map.SetTrack(x, y, tracksOwner);

        int regionId = _regionTracker.GetRegionId(x, y);

        _regionTracker.UpdateRegion(regionId, instability, inked);

        if (tracksOwner != -1)
        {
            _regionTracker.AddTrack(regionId, x, y, tracksOwner);
        }
    }

    internal void InitialiseCellToRegion(int x, int y, int regionId)
    {
        _regionTracker.AddCellToRegion(x, y, regionId);
    }

    internal void AddTownToRegion(int townId, int townX, int townY)
    {
        _regionTracker.AddTown(townId, townX, townY);
    }
}
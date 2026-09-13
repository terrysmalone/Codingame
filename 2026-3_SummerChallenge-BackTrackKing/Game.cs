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
    private ConnectionTracker _connectionTracker;

    public Game(int myId, int width, int height)
    {
        _myId = myId;

        _towns = new List<Town>();

        _regionTracker = new RegionTracker(_myId, width, height);
        _connectionTracker = new ConnectionTracker(width, height);
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
        // Logger.ConnectionScoresMap(_connectionTracker.GetConnectionScoresMap());
        Logger.LogTime($"Calculating desire paths with full unstable regions exclusion");
        List<DesirePath> desirePaths = CalculateDesirePaths(_regionTracker.GetExcludeUnstablePoints());
        Logger.LogTime($"Calculated {desirePaths.Count} fully excluded desire paths");

        if (desirePaths.Count == 0)
        {
            Logger.Message($"No desire paths found, calculating desire paths with inked only exclusions");
            desirePaths = CalculateDesirePaths(_regionTracker.GetExcludeInkedPoints());
            Logger.LogTime($"Calculated {desirePaths.Count} inked only excluded desire paths");
        }

        // Logger.DesirePaths(desirePaths);
        // _regionTracker.LogRegions();
        //_connectionTracker.LogConnections();

        TrackPlacementCalculator trackPlacementCalculator = new TrackPlacementCalculator(_map, _regionTracker);
        (var actions, var actionPointsLeft) =  trackPlacementCalculator.CalculateBestCandidates(desirePaths);

        Logger.LogTime($"Calculated cell placement");

        if (actionPointsLeft > 0)
        {
            actions += CalculateNextBestPaintActions(desirePaths, actionPointsLeft);

            Logger.LogTime($"Calculated next best actions");
        }

        actions += CalculateDisruptAction();

        Logger.LogTime($"Calculated disrupt actions");

        if (string.IsNullOrEmpty(actions))
        {
            actions = "WAIT;";
        }

        return actions;
    }

    private string CalculateNextBestPaintActions(List<DesirePath> desirePaths, int remainingActionPoints)
    {
        List<Point> paintedPoints = new List<Point>();

        if (remainingActionPoints > 0)
        {
            int getHighestAbsoluteScore = _connectionTracker.GetHighestAbsoluteConnectionScore();

            bool cutout = false;

            List<Point> towns = _towns.Select(t => new Point(t.X, t.Y)).ToList();

            while (remainingActionPoints > 0 && getHighestAbsoluteScore > 0 && !cutout)
            {
                for (int y= 0; y < _map.Height; y++)
                {
                    if (cutout)
                    {
                        break;
                    }
                    for (int x = 0; x < _map.Width; x++)
                    {
                        if (cutout)
                        {
                            break;
                        }
                        int score = Math.Abs(_connectionTracker.GetConnectionScoresMap()[x, y]);
                        if (score == getHighestAbsoluteScore)
                        {
                            var neighbours = new Point[4];
                            neighbours[0] = new Point(x, y - 1); // North
                            neighbours[1] = new Point(x + 1, y); // East
                            neighbours[2] = new Point(x, y + 1); // South
                            neighbours[3] = new Point(x - 1, y); // West

                            foreach (var pt in neighbours)
                            {
                                // bounds check
                                if (pt.X < 0 || pt.X >= _map.Width || pt.Y < 0 || pt.Y >= _map.Height)
                                {
                                    continue;
                                }

                                int regionId = _regionTracker.GetRegionId(pt.X, pt.Y);

                                if (_map.isTrackFree(pt.X, pt.Y) && !towns.Contains(pt) && !paintedPoints.Contains(pt) && !_regionTracker.IsRegionInked(regionId))
                                {   
                                    int cellValue = _map.CellCosts[pt.X, pt.Y];

                                    if (cellValue <= remainingActionPoints)
                                    {
                                        remainingActionPoints -= cellValue;
                                        paintedPoints.Add(pt);
                                        if (remainingActionPoints <= 0)
                                        {
                                            cutout = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                getHighestAbsoluteScore--;
            }
        }

        if (remainingActionPoints > 0)
        {
            Logger.Error($"Unspent action points: {remainingActionPoints}");
        }

        return GetActionsString(paintedPoints);
    }

    private static string GetActionsString(List<Point> actionPoints)
    {
        // Action points to a string of actions
        string actions = string.Empty;

        foreach (var point in actionPoints)
        {
            actions += $"PLACE_TRACKS {point.X} {point.Y};";
            Logger.Message($"Placed track:{point.X} {point.Y}, Priority: NextBestAction");
        }

        return actions;
    }

    private List<DesirePath> CalculateDesirePaths(HashSet<Point> excludePoints)
    {
        // Logger.Message("Calculating desire paths");

        List<DesirePath> desirePaths = new List<DesirePath>();
        foreach (var town in _towns)
        {
            foreach (var desiredConnection in town.DesiredConnections)
            {
                List<Point> fullSanitisedPath = FindShortestSanitisedPath(new Point(town.X, town.Y), desiredConnection, excludePoints);
                
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

                int myTracksOnPathCount = 0;
                int opponentTracksOnPathCount = 0;

                int highestActionCostOnRemainingPath = 0;

                foreach (var point in fullSanitisedPath)
                {
                    if (_map.isTrackFree(point.X, point.Y))
                    {
                        remainingPathPoints.Add(point);

                        // Track the highest action cost on the remaining path for later use in prioritisation
                        if (_map.CellCosts[point.X, point.Y] > highestActionCostOnRemainingPath)
                        {
                            highestActionCostOnRemainingPath = _map.CellCosts[point.X, point.Y];
                        }
                    }
                    else
                    {
                        int trackOwner = _map.GetTrackOwner(point.X, point.Y);

                        if (trackOwner != -1 && trackOwner != 2)
                        {
                            if (trackOwner == _myId)
                            {
                                myTracksOnPathCount++;
                            }
                            else
                            {
                                opponentTracksOnPathCount++;
                            }
                        }
                    }
                }

                int remainingPathCount = remainingPathPoints.Count;
                int remainingActionCount = CalculateActionCount(remainingPathPoints);

                var desirePath = new DesirePath(fullSanitisedPath, remainingPathPoints, $"{town.Id}-{desiredConnection}")
                {
                    FullPathCount = fullPathCount,
                    FullActionCount = fullActionCount,
                    RemainingPathCount = remainingPathCount,
                    RemainingActionCount = remainingActionCount,
                    MyTracksOnPathCount = myTracksOnPathCount,
                    OpponentTracksOnPathCount = opponentTracksOnPathCount,
                    HighestActionCostOnRemainingPath = highestActionCostOnRemainingPath
                };

                desirePaths.Add(desirePath);
            }
        }

        // Logger.Message($"Finished calculating {desirePaths.Count} desire paths");

        return desirePaths.OrderBy(dp => dp.RemainingActionCount).ThenBy(dp => dp.RemainingPathCount).ToList();
    }

    private List<Point> FindShortestSanitisedPath(Point startPoint, int desiredConnection, HashSet<Point> excludePoints)
    {
        Town desiredTown = _towns.First(t => t.Id == desiredConnection);

        List<Point> shortestPath = _pathFinder.GetShortestPath(new Point(startPoint.X, startPoint.Y), new Point(desiredTown.X, desiredTown.Y), excludePoints);

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
            int cellCost = _map.CellCosts[point.X, point.Y];
            actionCount += cellCost;
        }

        return actionCount;
    }

    private string CalculateDisruptAction()
    {
        int region = -1;
        // PLAN
        // NOTE: In most cases if we've started to disrupt a region then finish. Only point 1 should override that. 
        //       We want to always prioritise stopping the opponent from scoring
        //
        // Priorities
        // 1. Target regions that contain completed tracks generating the enemy the most points
        region = _regionTracker.GetStrongestEnemyRegionWithActiveTracks(_connectionTracker.GetConnectionScores());


        // 2. Target regions that contain the most partially completed tracks that belong to the enemy
        // 3. Target the region with the highest ratio of enemy tracks to my tracks
        if (region == -1)
        {
            region = _regionTracker.GetStrongestEnemyRegion();
        }

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

    internal void UpdateCell(int x, int y, int tracksOwner, int instability, bool inked, string[]? connections)
    {
        _map.SetTrack(x, y, tracksOwner);

        int regionId = _regionTracker.GetRegionId(x, y);

        _regionTracker.UpdateRegion(regionId, instability, inked);

        if (tracksOwner != -1)
        {
            _regionTracker.AddTrack(regionId, x, y, tracksOwner, connections);

            if (tracksOwner != 2)
            {
                bool myTrack = tracksOwner == _myId;
                
                _connectionTracker.UpdateConnections(connections, x, y, myTrack);
            }
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

    internal void ResetRegions()
    {
        _regionTracker.ResetRegions();
        _connectionTracker.ClearConnections();
    }
}
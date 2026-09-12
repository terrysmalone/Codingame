using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace BackTrackKing;

// Keeps track of scores for every cell in the game and works out which ones are best to place
//
// General strategy
//
// From all desire paths extract every viable cell. Store:
// Cell 
// { 
//    CellPosition,
//    Action Cost,
//    RegionID,
//    How many desire paths it is part of
//      Store all of these paths:
//        Full path points
//        Remaining points
//        Full path Action cost
//        Remaining path action cost
//        Highest instability level along this path
//        town join (2 to 6)
//    Is it in a safe region (safe regions contain town so can never be inked)
//    This cells instability level
//
// We want to prioritise in this order
// 1. If we can complete a desire path fully in this turn then do it. 
// 2. Place tracks on the lowest cost cells. Defined by:
//      a. plains > river > mountain
//      b. place cells in safe regions first
//      c. place cells in different regions and/or town-join to reduce the risk of being inked
internal class TrackPlacementCalculator
{
    private Dictionary<Point, TrackCandidate> _candidates;

    private Map _map;
    private RegionTracker _regionTracker;

    public TrackPlacementCalculator(Map map, RegionTracker regionTracker)
    {
        _candidates = new Dictionary<Point, TrackCandidate>();
        _map = map;
        _regionTracker = regionTracker;
    }

    public (string, int) CalculateBestCandidates(List<DesirePath> desirePaths)
    {
        string actions = string.Empty;
        int actionPointsLeft = 3;

        _candidates.Clear();
        HashSet<Point> placedCells = new HashSet<Point>();

        // Before doing anything, check if we can complete a desire path fully this turn.
        // If so, we should do that first. This is a higher priority than any other placement strategy.
        foreach (var desirePath in desirePaths)
        {
            // TODO: At some point lets check if we can complete multiple desire paths this turn. 
            // We should picj the best. Not just the first one
            if (desirePath.RemainingActionCount <= actionPointsLeft && DesirePathUtil.IsCompletionWorthwhile(desirePath))
            {
                // Get the actions for the remaining path
                foreach (var cellPosition in desirePath.RemainingPath)
                {
                    actions += $"PLACE_TRACKS {cellPosition.X} {cellPosition.Y};";
                    placedCells.Add(cellPosition);
                    actionPointsLeft -= _map.CellCosts[cellPosition.X, cellPosition.Y];
                }
            }
        }

        if (actionPointsLeft <= 0)
        {
            return (actions, actionPointsLeft);
        }


        FillCandidates(desirePaths);

        List<TrackCandidate> candidates = new List<TrackCandidate>(_candidates.Values);

                                                                                    // Priority order
        candidates = candidates.Where(c => c.IsPathWorthwhile)                      // Filter out candidates that aren't worthwhile    
                               .OrderBy(c => c.ShortestRemainingCount())            // Shortest to complete                                        
                               .ThenByDescending(c => c.GetTownCount())             // Number of desire paths this route passes through
                               .ThenBy(c => c.ActionCost)                           // Lowest cost first
                               .ThenByDescending(c => c.IsInSafeRegion).ToList();   // Safe regions first

        // Logger.TrackCandidates(candidates);

        HashSet<int> placedRegions = new HashSet<int>();
        HashSet<string> placedDesirePaths = new HashSet<string>();

        int timesChecked = 0;   // Do a maximum of 4 passes through the candidates to try and place tracks to avoid infinite loops

        while (actionPointsLeft > 0 && timesChecked < 4)
        {
            // Reset placedRegions every time we do another passthrough
            placedRegions.Clear();
            placedDesirePaths.Clear();

            foreach (var candidate in candidates)
            {
                if (candidate.ActionCost <= actionPointsLeft
                    && !placedRegions.Contains(candidate.RegionId)
                    && !placedCells.Contains(candidate.CellPosition)
                    && !HasCandidateRegionBeenPlaced(candidate, placedDesirePaths))
                {
                    actionPointsLeft -= candidate.ActionCost;
                    placedCells.Add(candidate.CellPosition);
                    placedRegions.Add(candidate.RegionId);
                    placedDesirePaths.UnionWith(candidate.DesirePaths.Select(dp => dp.TownConnection));
                    actions += $"PLACE_TRACKS {candidate.CellPosition.X} {candidate.CellPosition.Y};";
                }
            }

            timesChecked++;
        }

        return (actions, actionPointsLeft);
    }

    private void FillCandidates(List<DesirePath> desirePaths)
    {
        foreach (var desirePath in desirePaths)
        {
            foreach (var cellPosition in desirePath.RemainingPath)
            {
                TrackCandidate? candidate = null;

                if (_candidates.ContainsKey(cellPosition))
                {
                    candidate = _candidates[cellPosition];
                }
                else
                {
                    int regionId = _map.RegionIds[cellPosition.X, cellPosition.Y];
                    int actionCost = _map.CellCosts[cellPosition.X, cellPosition.Y];

                    bool isInSafeRegion = _regionTracker.IsSafeRegion(regionId);

                    int instabilityLevel = _regionTracker.GetInstabilityLevel(regionId);

                    candidate = new TrackCandidate(cellPosition, regionId, actionCost, isInSafeRegion, instabilityLevel);
                }

                candidate.AddDesirePath(desirePath);

                _candidates[cellPosition] = candidate;
            }
        }
    }

    private bool HasCandidateRegionBeenPlaced(TrackCandidate candidate, HashSet<string> placedDesirePaths)
    {
        foreach (var desirePath in candidate.DesirePaths)
        {
            if (placedDesirePaths.Contains(desirePath.TownConnection))
            {
                return true;
            }
        }

        return false;
    }
}

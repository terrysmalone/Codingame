using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace BackTrackKing;

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

        // Have two priorities and alternate between the two.
        // First, prioritise shortest first paths, then longest first paths.
        //

        List<TrackCandidate> candidates = new List<TrackCandidate>(_candidates.Values);

                                                                                    // Priority order
        candidates = candidates.Where(c => c.IsPathWorthwhile)                      // Filter out candidates that aren't worthwhile    
                               .OrderBy(c => c.ActionCost)                           // Lowest cost first
                               .ThenBy(c => c.ShortestRemainingCount())            // Shortest to complete    
                               .ThenByDescending(c => c.IsInSafeRegion).ToList();   // Safe regions first

        Logger.TrackCandidates(candidates);

        HashSet<int> placedRegions = new HashSet<int>();

        int timesChecked = 0;   // Do a maximum of 4 passes through the candidates to try and place tracks to avoid infinite loops

        while (actionPointsLeft > 0 && timesChecked < 4)
        {
            // Reset placedRegions every time we do another passthrough
            placedRegions.Clear();

            foreach (var candidate in candidates)
            {
                if (candidate.ActionCost <= actionPointsLeft && !placedRegions.Contains(candidate.RegionId) && !placedCells.Contains(candidate.CellPosition))
                {
                    actionPointsLeft -= candidate.ActionCost;
                    placedCells.Add(candidate.CellPosition);
                    placedRegions.Add(candidate.RegionId);
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
}

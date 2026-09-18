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

        // Before doing anything, check if we can complete any desire paths fully this turn.
        // If so, pick the best one. This is a higher priority than any other placement strategy.
        var completablePaths = desirePaths
            .Where(dp => dp.RemainingActionCount <= actionPointsLeft && DesirePathUtil.IsCompletionWorthwhile(dp))
            .OrderByDescending(dp => dp.FullPathCount) // Prioritise completing larger paths
            .ToList();

        foreach (var desirePath in completablePaths)
        {
            int trueRemainingCost = desirePath.RemainingPath
                .Where(p => !placedCells.Contains(p))
                .Sum(p => _map.CellCosts[p.X, p.Y]);

            // Can we afford to do this one
            if (trueRemainingCost > actionPointsLeft)
            {
                continue;
            }

            foreach (var cellPosition in desirePath.RemainingPath)
            {
                // Skip if already placed
                if (placedCells.Contains(cellPosition))
                {
                    continue; 
                }

                actions += $"PLACE_TRACKS {cellPosition.X} {cellPosition.Y};";
                placedCells.Add(cellPosition);
                actionPointsLeft -= _map.CellCosts[cellPosition.X, cellPosition.Y];
            }            
        }

        if (actionPointsLeft <= 0)
        {
            return (actions, actionPointsLeft);
        }


        FillCandidates(desirePaths);
        CalculatePotentialScoresThroughCells(desirePaths);

        List<TrackCandidate> candidates = new List<TrackCandidate>(_candidates.Values);

                                                                                    // Priority order
        candidates = candidates.Where(c => c.IsPathWorthwhile)                      // Filter out candidates that aren't worthwhile                                
                               .OrderBy(c => c.GetShortestRemainingActionCount())   // Shortest to complete 
                               .ThenBy(c => c.ActionCost)                           // Lowest cost first
                               .ThenByDescending(c => c.GetTownCount())             // Number of desire paths this route passes through
                               .ThenBy(c => c.InstabilityLevel)                     // Lowest instability level firs
                               .ThenByDescending(c => c.IsInSafeRegion).ToList();  // Safe regions first


        Logger.TrackCandidates(candidates);

        HashSet<int> placedRegions = new HashSet<int>();

        int timesChecked = 0;   // Do a maximum of 4 passes through the candidates to try and place tracks to avoid infinite loops

        while (actionPointsLeft > 0 && timesChecked < 4)
        {
            // Reset placedRegions every time we do another passthrough
            placedRegions.Clear();
            bool placedSomethingThisPass = false;

            foreach (var candidate in candidates)
            {
                if (candidate.ActionCost <= actionPointsLeft && !placedRegions.Contains(candidate.RegionId) && !placedCells.Contains(candidate.CellPosition))
                {
                    actionPointsLeft -= candidate.ActionCost;
                    placedCells.Add(candidate.CellPosition);
                    //placedRegions.Add(candidate.RegionId); // TODO: If we don't want to use this remove it properly
                    actions += $"PLACE_TRACKS {candidate.CellPosition.X} {candidate.CellPosition.Y};";
                    placedSomethingThisPass = true;
                }
            }

            // No progress is possible. Stop looping
            if (!placedSomethingThisPass)
            {
                break; 
            }

            timesChecked++;
        }

        return (actions, actionPointsLeft);
    }

    private void CalculatePotentialScoresThroughCells(List<DesirePath> desirePaths)
    {
        foreach (var desirePath in desirePaths)
        {
            int potentialScoreInDesirePath = desirePath.MyTracksOnPathCount + desirePath.RemainingPathCount;

            foreach (var cellPosition in desirePath.RemainingPath)
            {
                if (_candidates.ContainsKey(cellPosition))
                {
                    _candidates[cellPosition].PotentialScoresthroughCells = 
                        _candidates[cellPosition].PotentialScoresthroughCells + potentialScoreInDesirePath;
                }
                else
                {
                    Logger.Error($"Candidate not found for cell position {cellPosition.X}, {cellPosition.Y} when calculating potential scores through cells.");
                }
            }
        }
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

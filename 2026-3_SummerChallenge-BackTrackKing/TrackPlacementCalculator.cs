using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection.Metadata;

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

    public void CalculateBestCandidates(List<DesirePath> desirePaths)
    {
        _candidates.Clear();

        // Before doing anything, check if we can complete a desire path fully this turn.
        // If so, we should do that first. This is a higher priority than any other placement strategy.
        // Open question: Should we check instability levels of the desire paths for this? THis won'r matter at first because 
        // We currently exclude any amout of instability from the path finding search. At some point we'll change this

        FillCandidates(desirePaths);

        Logger.TrackCandidates(_candidates);
        
        // Group candidates by region and/or town join
        // Get the best scoring candidates. Note, don't take them all from the same one, unless they're a lot stronger

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

using System;
using System.Collections.Generic;
using System.Drawing;

namespace BackTrackKing;

internal class TrackCandidate
{
    internal Point CellPosition { get; private set; }

    internal int RegionId { get; set; }

    internal int ActionCost { get; set; }

    internal int DesirePathCount { get; set; }

    internal List<DesirePath> DesirePaths { get; set; }

    internal bool IsInSafeRegion { get; set; }

    internal int InstabilityLevel { get; set; }

    internal bool IsPathWorthwhile { get; set; } = true;

    private HashSet<string> _towns = new HashSet<string>();

    private int _shortestRemainingCount = int.MaxValue;

    internal TrackCandidate(Point cellPosition, int regionId, int actionCost, bool isInSafeRegion, int instabilityLevel)
    {
        CellPosition = cellPosition;
        RegionId = regionId;
        ActionCost = actionCost;
        DesirePaths = new List<DesirePath>();
        IsInSafeRegion = isInSafeRegion;
        InstabilityLevel = instabilityLevel;
    }

    internal void AddDesirePath(DesirePath desirePath)
    {
        DesirePaths.Add(desirePath);

        DesirePathCount++;

        // Add to towns list
        _towns.Add(desirePath.TownConnection);

        if (desirePath.RemainingActionCount < _shortestRemainingCount)
        {
            _shortestRemainingCount = desirePath.RemainingActionCount;
        }

        if (!DesirePathUtil.IsCompletionWorthwhile(desirePath))
        {
            IsPathWorthwhile = false;
        }

    }

    internal int GetTownCount()
    {
        return _towns.Count;
    }

    internal int ShortestRemainingCount()
    {
        return _shortestRemainingCount;
    }
}
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


    public TrackCandidate(Point cellPosition, int regionId, int actionCost, bool isInSafeRegion, int instabilityLevel)
    {
        CellPosition = cellPosition;
        RegionId = regionId;
        ActionCost = actionCost;
        DesirePaths = new List<DesirePath>();
        IsInSafeRegion = isInSafeRegion;
        InstabilityLevel = instabilityLevel;
    }

    public void AddDesirePath(DesirePath desirePath)
    {
        DesirePaths.Add(desirePath);
        DesirePathCount++;
    }
}

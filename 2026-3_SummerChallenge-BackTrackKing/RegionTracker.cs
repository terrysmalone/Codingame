using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace BackTrackKing;

internal class RegionTracker
{
    private List<Region> _regions;

    public RegionTracker()
    {
        _regions = new List<Region>();
    }

    internal void AddCellToRegion(int x, int y, int regionId)
    {
        Region? existingRegion = _regions.SingleOrDefault(r => r.Id == regionId);

        if (existingRegion == null)
        {
            Region region = new Region(regionId);
            region.AddCell(x, y);
            _regions.Add(region);
        }
        else
        {
            existingRegion.AddCell(x, y);
        }
    }

    internal HashSet<Point> GetExcludePoints()
    {
        HashSet<Point> excludePoints = new HashSet<Point>();

        foreach (var region in _regions)
        {
            if (region.IsInked)
            {
                excludePoints.UnionWith(region.GetCells());
            }
        }

        return excludePoints;
    }

    internal int GetRegionId(int x, int y)
    {
        Region? region = _regions.SingleOrDefault(r => r.GetCells().Contains(new Point(x, y)));

        if (region == null)
        {
            Logger.Error($"Region not found for cell ({x}, {y}) in GetRegionId");
            return -1;
        }

        return region.Id;
    }

    internal void AddTrack(int regionId, int x, int y, int tracksOwner)
    {
        Region? region = _regions.SingleOrDefault(r => r.GetCells().Contains(new Point(x, y)));

        if (region == null)
        {
            Logger.Error($"Region not found for cell ({x}, {y}) in AddTrack");
        }

        region.AddTrack(x, y, tracksOwner);
    }

    internal void UpdateRegion(int regionId, int instability, bool inked)
    {
        Region? region = _regions.SingleOrDefault(r => r.Id == regionId);

        if (region == null)
        {
            Logger.Error($"Region not found for id {regionId} in UpdateRegion");
        }

        region.UpdateInstability(instability, inked);
    }
}
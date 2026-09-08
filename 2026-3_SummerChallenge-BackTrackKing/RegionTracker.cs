using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace BackTrackKing;

internal class RegionTracker
{
    private int _myId;
    private List<Region> _regions;

    public RegionTracker(int myId)
    {
        _myId = myId;
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
            return;
        }

        switch (tracksOwner)
        {
            case 0:
                region.AddMyTrack(x, y);
                break;
            case 1:
                region.AddOpponentTrack(x, y);
                break;
            case 2:
                region.AddJointTrack(x, y);
                break;
            default:
                Logger.Error($"Invalid tracksOwner value {tracksOwner} in AddTrack");
                break;
        }
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

    // First pass at getting a disrupt action
    // For every region calculate enemyTracks - myTracks, Choose the region with the highest
    // score
    internal int GetStrongestEnemyRegion()
    {
        // For every region calculate enemyTracks -myTracks, Choose the region with the highest score
        int strongestEnemyRegion = int.MinValue;
        int strongerstEnemyRegionId = -1;

        foreach (Region region in _regions) 
        {
            if (region.IsInked)
            {
                continue;
            }

            int score = region.GetEnemyTracks() - region.GetMyTracks();

            if (score > strongestEnemyRegion)
            {
                strongestEnemyRegion = score;
                strongerstEnemyRegionId = region.Id;
            }
        }

        return strongerstEnemyRegionId;
    }
}
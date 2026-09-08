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

        if (tracksOwner == -1)
        {
            return;
        }

        if (tracksOwner == 2)
        {
            region.AddJointTrack(x, y);
        }

        if (_myId == 0)
        {
            if (tracksOwner == 0)
            {
                region.AddMyTrack(x, y);
            }
            else if (tracksOwner == 1)
            {
                region.AddOpponentTrack(x, y);
            }
        }
        else
        {
            if (tracksOwner == 0)
            {
                region.AddOpponentTrack(x, y);                
            }
            else if (tracksOwner == 1)
            {
                region.AddMyTrack(x, y);
            }
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
            if (region.IsInked || region.HasTown)
            {
                continue;
            }

            int score = region.GetEnemyTracks() - region.GetMyTracks();

            if (score > strongestEnemyRegion && score > 0)
            {
                strongestEnemyRegion = score;
                strongerstEnemyRegionId = region.Id;
            }
        }

        return strongerstEnemyRegionId;
    }

    internal void AddTown(int townId, int townX, int townY)
    {
        Region? region = _regions.SingleOrDefault(r => r.GetCells().Contains(new Point(townX, townY)));

        if (region == null)
        {
            Logger.Error($"Region not found for cell ({townX}, {townY}) in AddTown");
            return;
        }

        region.HasTown = true;
    }
}
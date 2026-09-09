using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace BackTrackKing;

internal class RegionTracker
{
    private int _myId;
    private List<Region> _regions;

    private Dictionary<int, int> _activeRegionScores;

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

    internal void AddTrack(int regionId, int x, int y, int tracksOwner, string[]? connections)
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

        region.AddActiveConnections(connections);

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

    internal int GetStrongestEnemyRegionWithActiveTracks(Dictionary<string, int> dictionary)
    {
        _activeRegionScores = new Dictionary<int, int>();

        foreach (Region region in _regions)
        {
            if (region.IsInked || region.HasTown)
            {
                continue;
            }

            int regionScore = 0;

            HashSet<string> regionConnections = region.GetActiveConnections();

            foreach (string connection in regionConnections)
            {
                if (dictionary.TryGetValue(connection, out int score))
                {
                    regionScore += score;
                }
            }

            _activeRegionScores.Add(region.Id, regionScore);
        }

        _activeRegionScores = _activeRegionScores.OrderByDescending(kv => kv.Value).ToDictionary(kv => kv.Key, kv => kv.Value);


        if (_activeRegionScores.Count > 0 && _activeRegionScores.First().Key > 0)
        {
            // TODO: Something about sticking with a region if we've started destabilising it already
            // Check all regions with the same high score. Pick the one with the highest instability
            int highScore = _activeRegionScores.First().Key;

            int highestInstability = int.MinValue;
            int highestInstabilityRegionId = -1;

            foreach (var regionScore in _activeRegionScores)
            {
                if (regionScore.Value == highScore)
                {
                    Region? region = _regions.SingleOrDefault(r => r.Id == regionScore.Key);
                    
                    if (region != null && region.Instability > highestInstability)
                    {
                        Logger.Message($"Checking region score for {region.Id} - {region.Instability}");
                        highestInstability = region.Instability;
                        highestInstabilityRegionId = region.Id;
                    }
                }
                else
                {
                    break;
                }
            }

            // Logger.RegionScores(_activeRegionScores);


            return highestInstabilityRegionId;
        }
        else
        {
            return -1;
        }
    }

    // First pass at getting a disrupt action
    // For every region calculate enemyTracks - myTracks, Choose the region with the highest
    // score
    internal int GetStrongestEnemyRegion()
    {
        // For every region calculate enemyTracks - myTracks, Choose the region with the highest score
        int strongestEnemyRegion = int.MinValue;
        int strongerstEnemyRegionId = -1;

        foreach (Region region in _regions)
        {
            if (region.IsInked || region.HasTown)
            {
                continue;
            }

            // Don't check this if the region has active connections in my favour
            int activeRegionScore = _activeRegionScores[region.Id];
            if(activeRegionScore < 0)
            {
                continue;
            }

            int score = region.GetEnemyTracks() - region.GetMyTracks();

            // If it's already been attacked, and it favours the enemy attack here straight away. 
            // Lets finish what we started.
            if (score > 0 && region.Instability > 0)
            {
                return region.Id;
            }

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

    internal void ResetRegions()
    {
        _regions.ForEach(r => r.ResetCounts());
    }

    internal void LogRegions()
    {
        Logger.Regions(_regions);
    }
}

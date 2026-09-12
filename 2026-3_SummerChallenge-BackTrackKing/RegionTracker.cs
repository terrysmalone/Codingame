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

    private int[,] _regionIds;

    private Dictionary<int, Region> _regionsById = new Dictionary<int, Region>();

    public RegionTracker(int myId, int width, int height)
    {
        _myId = myId;
        _regions = new List<Region>();
        _regionIds = new int[width, height];
    }

    internal void AddCellToRegion(int x, int y, int regionId)
    {
        _regionIds[x, y] = regionId;

        if (!_regionsById.TryGetValue(regionId, out var region))
        {
            region = new Region(regionId);
            _regions.Add(region);
            _regionsById[regionId] = region;
        }

        region.AddCell(x, y);
    }

    internal HashSet<Point> GetExcludePoints()
    {
        HashSet<Point> excludePoints = new HashSet<Point>();

        foreach (var region in _regions)
        {
            if (region.Instability > 0)
            {
                excludePoints.UnionWith(region.GetCells());
            }
        }

        return excludePoints;
    }

    internal int GetRegionId(int x, int y)
    {
        return _regionIds[x, y];
    }

    internal bool IsRegionInked(int regionId)
    {
        _regionsById.TryGetValue(regionId, out var region);

        if (region == null)
        {
            Logger.Error($"Region not found for id {regionId} in IsRegionInked");
            return false;
        }
        
        return region.IsInked;
    }

    internal void AddTrack(int regionId, int x, int y, int tracksOwner, string[]? connections)
    {
        _regionsById.TryGetValue(regionId, out var region);

        if (region == null)
        {
            Logger.Error($"Region not found for id {regionId} in AddTrack");
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
        _regionsById.TryGetValue(regionId, out var region);

        if (region == null)
        {
            Logger.Error($"Region not found for id {regionId} in UpdateRegion");
            return;
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

        if (_activeRegionScores.Count > 0 && _activeRegionScores.First().Value > 0)
        {
            // Logger.RegionScores(_activeRegionScores);

            int highScore = _activeRegionScores.First().Value;

            int highestInstability = int.MinValue;
            List<int> highestInstabilityRegionIds = new List<int>();

            foreach (var regionScore in _activeRegionScores)
            {
                if (regionScore.Value == highScore)
                {
                    _regionsById.TryGetValue(regionScore.Key, out var region);

                    if (region == null)
                    {
                        Logger.Error($"Region not found for id {regionScore.Key} in GetStrongestEnemyRegionWithActiveTracks");
                        break;
                    }
                    
                    if (region.Instability > highestInstability)
                    {
                        highestInstability = region.Instability;
                        highestInstabilityRegionIds.Clear();
                        highestInstabilityRegionIds.Add(region.Id);
                    }
                    else if (region.Instability == highestInstability)
                    {
                        highestInstabilityRegionIds.Add(region.Id);
                    }
                }
                else
                {
                    break;
                }
            }

            if (highestInstabilityRegionIds.Count == 1)
            {
                return highestInstabilityRegionIds.First();
            }
            else
            {
                // If there are multiple regions with the same high score and instability, choose the one with the most enemy tracks on it
                int mostEnemyTracks = int.MinValue;
                int mostEnemyTracksRegionId = -1;

                foreach (int regionId in highestInstabilityRegionIds)
                {
                    _regionsById.TryGetValue(regionId, out var region);

                    if (region == null)
                    {
                        Logger.Error($"Region not found for id {regionId} in GetStrongestEnemyRegionWithActiveTracks");
                        return -1;
                    }
                   
                    int enemyTracks = region.GetEnemyTracks();
                    if (enemyTracks > mostEnemyTracks)
                    {
                        mostEnemyTracks = enemyTracks;
                        mostEnemyTracksRegionId = region.Id;
                    }                    
                }

                return mostEnemyTracksRegionId;
            }
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
        int regionId = _regionIds[townX, townY];

        _regionsById.TryGetValue(regionId, out var region);

        if (region == null)
        {
            Logger.Error($"Region not found for id {regionId} in AddTown");
            return;
        }

        region.HasTown = true;
    }

    internal bool IsSafeRegion(int regionId)
    {
        _regionsById.TryGetValue(regionId, out var region);

        if (region == null)
        {
            Logger.Error($"Region not found for id {regionId} in IsSafeRegion");
            return false;
        }

        return region.HasTown;
    }

    internal int GetInstabilityLevel(int regionId)
    {
        _regionsById.TryGetValue(regionId, out var region);

        if (region == null)
        {
            Logger.Error($"Region not found for id {regionId} in GetInstabilityLevel");
            return -1;
        }

        return region.Instability;
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

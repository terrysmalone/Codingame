using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;

namespace BackTrackKing;

internal class RegionTracker
{
    private int _myId;
    private List<Region> _regions;

    private HashSet<RegionScore> _regionScores;

    private int[,] _regionIds;

    private Dictionary<int, Region> _regionsById = new Dictionary<int, Region>();
    private readonly float INK_CUTOFF = 4;

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

    internal HashSet<Point> GetExcludeUnstablePoints()
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

    internal HashSet<Point> GetExcludeInkedPoints()
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

    internal int GetStrongestEnemyRegion(Dictionary<string, int> connectionScores, List<DesirePath> desirePaths)
    {
        _regionScores = new HashSet<RegionScore>();

        // As well as an ActiveConnectionsScore we want to give it a non active connections score
        // Active connections score == points for each active connection that passes through that region
        // Non active connection score == points for each non active connection that passes through that region
        // We'll likely add these together after finding a way to weight them against each other
        //
        // Active connection score
        // For each active connection that passes throught the region (enemy track - my tracks)
        // Non active connection score
        // For each desire path that passes through the region (enemy track - my tracks)

        foreach (Region region in _regions)
        {
            if (region.IsInked || region.HasTown || region.GetEnemyTracks() == 0)
            {
                continue;
            }

            int activeConnectionsTracksScore = CalculateActiveConnectionsTracksScore(region, connectionScores);

            float nonActiveConnectionsScore = CalculateNonActiveConnectionsScore(region, desirePaths);

            var regionScore = new RegionScore(region.Id, activeConnectionsTracksScore);

            regionScore.myTracksCount = region.GetMyTracks();
            regionScore.enemyTracksCount = region.GetEnemyTracks();

            regionScore.Instability = region.Instability;
            
            regionScore.ActiveConnectionsScore = (float)activeConnectionsTracksScore / (INK_CUTOFF - (float)regionScore.Instability);
            regionScore.NonActiveConnectionsScore = (float)nonActiveConnectionsScore;

            _regionScores.Add(regionScore);
        }

        var sortedRegionScores = 
            _regionScores.OrderByDescending(r => r.ActiveConnectionsScore)
                         .ThenByDescending(r => r.NonActiveConnectionsScore)
                         .ThenByDescending(r => r.ActiveConnectionsTracksScore)                         
                         .ThenByDescending(r => r.enemyTracksCount - r.myTracksCount)
                         .ThenByDescending(r => r.Instability)
                         .ToList();

        Logger.RegionScores(sortedRegionScores);

        return sortedRegionScores.Count > 0 ? sortedRegionScores[0].Id : -1;
    }

    private int CalculateActiveConnectionsTracksScore(Region region, Dictionary<string, int> connectionScores)
    {
        int activeConnectionsTracksScore = 0;

        // Get all active connections passing through this region
        HashSet<string> activeRegionConnections = region.GetActiveConnections();

        foreach (string connection in activeRegionConnections)
        {
            if (connectionScores.TryGetValue(connection, out int score))
            {
                activeConnectionsTracksScore += score;
            }
        }

        return activeConnectionsTracksScore;
    }

    private float CalculateNonActiveConnectionsScore(Region region, List<DesirePath> desirePaths)
    {
        float nonActiveConnectionsScore = 0;
        foreach (DesirePath desirePath in desirePaths)
        {
            if (!desirePath.GetRegionIds().Contains(region.Id))
            {
                continue;
            }

            int opponentAdvantage = desirePath.OpponentTracksOnPathCount - desirePath.MyTracksOnPathCount;

            if (opponentAdvantage <= 0)
            {
                continue;
            }

            // Proportion of the path already completed (0 = nothing built, close to 1 = nearly finished)
            float completionRatio = desirePath.FullActionCount > 0
                ? 1f - ((float)desirePath.RemainingActionCount / desirePath.FullActionCount)
                : 0f;

            // Weight advantage by how close to completion the path is
            nonActiveConnectionsScore += opponentAdvantage * (completionRatio * completionRatio);
        }
        return nonActiveConnectionsScore;
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

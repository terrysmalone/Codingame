using System.Collections.Generic;

namespace BackTrackKing;

internal class RegionScore
{
    internal int Id { get; private set; }
    
    internal int ActiveConnectionCount 
    { 
        get =>  ActiveRegionConnections.Count;
    }
    internal int ActiveConnectionsTracksScore { get; set; }

    internal HashSet<string> ActiveRegionConnections { get; private set; }

    internal int myTracksCount { get; set; }
    internal int enemyTracksCount { get; set; }
    public int Instability { get; internal set; }
    public float RegionEfficiencyScore { get; internal set; }

    public RegionScore(int id, HashSet<string> regionConnections, int activeConnectionsTracksScore)
    {
        Id = id;
        ActiveRegionConnections = regionConnections;
        ActiveConnectionsTracksScore = activeConnectionsTracksScore;
    }
}

using System.Collections.Generic;

namespace BackTrackKing;

internal class RegionScore
{
    internal int Id { get; private set; }
    
    internal int ActiveConnectionsTracksScore { get; set; }

    internal int myTracksCount { get; set; }
    internal int enemyTracksCount { get; set; }
    public int Instability { get; internal set; }
    public float ActiveConnectionsScore { get; internal set; }

    public float NonActiveConnectionsScore { get; internal set; }

    public RegionScore(int id, int activeConnectionsTracksScore)
    {
        Id = id;
        ActiveConnectionsTracksScore = activeConnectionsTracksScore;
    }
}

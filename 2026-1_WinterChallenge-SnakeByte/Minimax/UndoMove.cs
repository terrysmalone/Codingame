using System.Collections.Generic;
using System.Drawing;

namespace _2026_1_WinterChallenge_SnakeByte;

internal struct UndoMove
{
    public SnakeChange[] Changes;
    public HashSet<Point> EatenPowerUps;
}

internal struct SnakeChange
{
    internal int Id;
    internal bool Moved;
    internal bool Grew;
    internal Point RemovedTail;
    internal int GravityFall;
    internal bool Killed;
    internal List<Point>? KilledBody;
    internal Point CollisionHead;
    internal bool LostCollisionSegment;
}

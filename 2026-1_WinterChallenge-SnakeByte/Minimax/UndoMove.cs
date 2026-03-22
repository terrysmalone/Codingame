using System.Collections.Generic;
using System.Drawing;

namespace _2026_1_WinterChallenge_SnakeByte;

internal struct UndoMove
{
    public List<(int Id, List<Point> OldBody)> SnakeBodies;
    public HashSet<Point> EatenPowerUps;
}

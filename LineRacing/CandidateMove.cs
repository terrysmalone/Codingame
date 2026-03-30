using System.Collections.Generic;
using System.Drawing;

namespace LineRacing;

internal class CandidateMove
{
    internal Point Move { get; set; }
    internal List<Point> Beam { get; set; }

    internal int MySpace { get; set; }
    internal int EnemySpace { get; set; }

    internal int Score { get; set; }
}

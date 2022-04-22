using System.Drawing;
using System.Runtime.InteropServices;

namespace SpringChallenge2022;

internal sealed class Hero
{
    public int Id { get; }
    public Point Position { get; set; }

    public Point GuardPoint { get; set; }

    internal int CurrentMonster { get; set; } = -1;

    internal string CurrentAction { get; set; } = "WAIT";

    internal Strategy Strategy { get; set;} = Strategy.Defend;

    public Hero(int id, Point position)
    {
        Id = id;
        Position = position;
    }
}

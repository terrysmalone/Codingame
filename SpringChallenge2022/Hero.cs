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

    internal bool UsingSpell {get; set; } = false;

    internal bool IsControlled { get; set; } = false;

    internal int ShieldLife { get; set; }

    internal Strategy Strategy { get; set;} = Strategy.Defend;
    internal  bool IsShielding { get; set; }

    public Hero(int id, Point position, bool isControlled, int shieldLife)
    {
        Id = id;
        Position = position;
        IsControlled = isControlled;
        ShieldLife = shieldLife;
    }
}

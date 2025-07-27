
using System.Drawing;

namespace SpringChallenge2020;

internal class Pac
{
    internal int Id;
    internal Point Position;
    internal string TypeId;
    internal int SpeedTurnsLeft;
    internal int AbilityCooldown;

    internal bool TargetSet;

    public Pac(int id, Point position, string typeId, int speedTurnsLeft, int abilityCooldown)
    {
        Id = id;
        Position = position;
        TypeId = typeId;
        SpeedTurnsLeft = speedTurnsLeft;
        AbilityCooldown = abilityCooldown;
    }
}

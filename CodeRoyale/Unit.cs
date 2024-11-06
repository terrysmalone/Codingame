namespace CodeRoyale;
    
using System.Drawing;

internal sealed class Unit
{
    internal UnitType Type { get; }
    internal Point Position { get; }
    internal int Health { get; }
    internal Unit(UnitType type,
        Point position,
        int health)
    {
        Type = type;
        Position = position;
        Health = health;
    }
}
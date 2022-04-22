using System.Drawing;

namespace SpringChallenge2022;

internal sealed class Monster
{
    public int Id { get; }
    public Point Position { get; }
    public int Health { get; }
    public int SpeedX { get; }
    public int SpeedY { get; }
    public bool NearBase { get; }
    public ThreatFor ThreatFor { get; }
    public int ShieldLife { get; }
    public bool IsControlled { get; }
    
    public Monster(int id, Point position, int health, int speedX, int speedY, bool nearBase, ThreatFor threatFor, bool isControlled, int shieldLife)
    {
        Id = id;
        Position = position;
        Health = health;
        SpeedX = speedX;
        SpeedY = speedY;
        NearBase = nearBase;
        ThreatFor = threatFor;
        IsControlled = isControlled;
        ShieldLife = shieldLife;
    }
}

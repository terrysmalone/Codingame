using System.Drawing;

namespace SpringChallenge2022;

internal sealed class Monster
{
    public int Id { get; }
    public Point Position { get; }
    public int Health { get; }
    public int XTrajectory { get; }
    public int YTrajectory { get; }
    public bool NearBase { get; }
    public ThreatFor ThreatFor { get; }
    public bool IsControlled { get; }
    
    public Monster(int id, Point position, int health, int xTrajectory, int yTrajectory, bool nearBase, ThreatFor threatFor, bool isControlled)
    {
        Id = id;
        Position = position;
        Health = health;
        XTrajectory = xTrajectory;
        YTrajectory = yTrajectory;
        NearBase = nearBase;
        ThreatFor = threatFor;
        IsControlled = isControlled;
    }
}

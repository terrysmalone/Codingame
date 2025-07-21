using System.Drawing;

namespace SummerChallenge2025_SoakOverflow;

class Agent
{
    public int Id { get; private set; }
    public int Player { get; private set; }
    public int ShootCooldown { get; set; }
    public int OptimalRange { get; private set; }
    public int SoakingPower { get; private set; }
    public int SplashBombs { get; set; }
    public int Wetness { get; set; }

    public Priority AgentPriority { get; set; } = Priority.MovingToEnemy;

    public Point Position { get; private set; } = new Point(-1, -1);

    public bool InGame { get; set; } = false;

    public Agent(int id, int player, int shootCooldown, int optimalRange, int soakingPower, int splashBombs)
    {
        Id = id;
        Player = player;
        ShootCooldown = shootCooldown;
        OptimalRange = optimalRange;
        SoakingPower = soakingPower;
        SplashBombs = splashBombs;
    }

    internal void UpdatePosition(int x, int y)
    {
        Position = new Point(x, y);
    }
}

public enum Priority
{
    MovingToEnemy,
    FindingBestAttackPosition,
}
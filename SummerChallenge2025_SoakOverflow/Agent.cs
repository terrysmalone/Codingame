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
    public Point Position { get; private set; } = new Point(-1, -1);
    public Priority AgentPriority { get; set; } = Priority.MovingToEnemy;
    public MoveIntention MoveIntention { get; set; } = new MoveIntention();
    public ActionIntention ActionIntention { get; set; } = new ActionIntention();

    public int ShootToKillId { get; set; } = -1; // Id of the agent to shoot to kill, -1 if no target
    public int ShootToSoakId { get; set; } = -1; // Id of the agent to shoot to soak, -1 if no target
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

    internal void ResetIntentions()
    {
        MoveIntention = new MoveIntention();
        ActionIntention = new ActionIntention();

        ShootToKillId = -1;
        ShootToSoakId = -1;
    }
}